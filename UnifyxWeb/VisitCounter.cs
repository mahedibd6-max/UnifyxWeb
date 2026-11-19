using Microsoft.EntityFrameworkCore;
using UnifyxWeb.Data;

namespace UnifyxWeb;

// Counts website visits per day: page views, different visitors and first-time visitors.
// Only public pages (/en, /bn …) that open successfully; Admin pages, files and search-engine bots are skipped.
// A visitor is recognised by two small cookies holding a random id and the last day seen — no name, no IP is stored.
public class VisitCounter(RequestDelegate next, ILogger<VisitCounter> log)
{
    private const string IdCookie = "ux-vid";
    private const string DayCookie = "ux-vday";
    private static readonly string[] Bots = { "bot", "spider", "crawl", "slurp", "preview", "facebookexternalhit", "headless", "lighthouse", "curl", "wget", "python" };

    public async Task InvokeAsync(HttpContext ctx, SiteDbContext db)
    {
        bool count = ShouldCount(ctx.Request);
        bool newVisitor = false, firstToday = false;
        string today = DateTime.Today.ToString("yyyyMMdd");
        if (count)
        {
            var opts = new CookieOptions { HttpOnly = true, Secure = ctx.Request.IsHttps, SameSite = SameSiteMode.Lax, IsEssential = true, Expires = DateTimeOffset.Now.AddYears(1) };
            if (!ctx.Request.Cookies.ContainsKey(IdCookie)) { newVisitor = true; ctx.Response.Cookies.Append(IdCookie, Guid.NewGuid().ToString("N"), opts); }
            if (ctx.Request.Cookies[DayCookie] != today) { firstToday = true; ctx.Response.Cookies.Append(DayCookie, today, opts); }
        }

        await next(ctx);

        if (!count || ctx.Response.StatusCode != 200) return;
        try
        {
            await AddAsync(db, DateTime.Today, Normalise(ctx.Request.Path.Value ?? "/"), firstToday, newVisitor);
        }
        catch (Exception ex) { log.LogWarning(ex, "Visit not counted."); }   // never break the page
    }

    private static bool ShouldCount(HttpRequest r)
    {
        if (!HttpMethods.IsGet(r.Method)) return false;
        string path = r.Path.Value ?? "";
        if (!(path == "/en" || path == "/bn" || path.StartsWith("/en/") || path.StartsWith("/bn/"))) return false;
        if (Path.HasExtension(path)) return false;
        string ua = r.Headers.UserAgent.ToString().ToLowerInvariant();
        if (ua.Length == 0 || Bots.Any(ua.Contains)) return false;
        return true;
    }

    // "/bn/News/Details/3" and "/en/News/Details/3" count as the same page.
    private static string Normalise(string path)
    {
        string rest = path.Length > 3 ? path[3..] : "/";
        if (rest.Length == 0) rest = "/";
        return rest.Length > 200 ? rest[..200] : rest;
    }

    private static async Task AddAsync(SiteDbContext db, DateTime day, string path, bool visitor, bool newVisitor)
    {
        int v = visitor ? 1 : 0, nv = newVisitor ? 1 : 0;
        int updated = await db.VisitDays.Where(x => x.Date == day).ExecuteUpdateAsync(s => s
            .SetProperty(x => x.PageViews, x => x.PageViews + 1)
            .SetProperty(x => x.Visitors, x => x.Visitors + v)
            .SetProperty(x => x.NewVisitors, x => x.NewVisitors + nv));
        if (updated == 0)
        {
            db.VisitDays.Add(new VisitDay { Date = day, PageViews = 1, Visitors = v, NewVisitors = nv });
            try { await db.SaveChangesAsync(); }
            catch (DbUpdateException)   // another request made today's row first
            {
                db.ChangeTracker.Clear();
                await db.VisitDays.Where(x => x.Date == day).ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.PageViews, x => x.PageViews + 1)
                    .SetProperty(x => x.Visitors, x => x.Visitors + v)
                    .SetProperty(x => x.NewVisitors, x => x.NewVisitors + nv));
            }
        }

        int pageUpdated = await db.VisitPages.Where(x => x.Date == day && x.Path == path)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Views, x => x.Views + 1));
        if (pageUpdated == 0)
        {
            db.VisitPages.Add(new VisitPage { Date = day, Path = path, Views = 1 });
            try { await db.SaveChangesAsync(); }
            catch (DbUpdateException)
            {
                db.ChangeTracker.Clear();
                await db.VisitPages.Where(x => x.Date == day && x.Path == path)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.Views, x => x.Views + 1));
            }
        }
    }
}
