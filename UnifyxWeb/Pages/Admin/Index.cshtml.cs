using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UnifyxWeb.Data;

namespace UnifyxWeb.Pages.Admin;

public class IndexModel : PageModel
{
    private readonly SiteDbContext _db;
    public IndexModel(SiteDbContext db) => _db = db;

    public int News { get; set; }
    public int ActivePopups { get; set; }
    public int NewRequests { get; set; }

    // Visitors
    public record Sum(int Views, int Visitors, int NewVisitors);
    public Sum Today { get; set; } = new(0, 0, 0);
    public Sum Yesterday { get; set; } = new(0, 0, 0);
    public Sum Last7 { get; set; } = new(0, 0, 0);
    public Sum ThisMonth { get; set; } = new(0, 0, 0);
    public Sum AllTime { get; set; } = new(0, 0, 0);
    public List<VisitDay> Last30 { get; set; } = new();
    public List<(string Path, int Views)> TopPages { get; set; } = new();
    public DateTime? Since { get; set; }

    public async Task OnGetAsync()
    {
        var today = DateTime.Today;
        News = await _db.NewsPosts.CountAsync(x => x.IsPublished);
        ActivePopups = await _db.Popups.CountAsync(x => x.IsActive && x.StartDate <= today && x.EndDate >= today);
        NewRequests = await _db.DemoRequests.CountAsync(x => !x.Contacted);

        var from30 = today.AddDays(-29);
        var days = await _db.VisitDays.AsNoTracking().Where(x => x.Date >= from30).ToListAsync();
        Last30 = Enumerable.Range(0, 30).Select(i => from30.AddDays(i))
            .Select(d => days.FirstOrDefault(x => x.Date == d) ?? new VisitDay { Date = d }).ToList();
        static Sum Of(IEnumerable<VisitDay> list) => new(list.Sum(x => x.PageViews), list.Sum(x => x.Visitors), list.Sum(x => x.NewVisitors));
        Today = Of(Last30.Where(x => x.Date == today));
        Yesterday = Of(Last30.Where(x => x.Date == today.AddDays(-1)));
        Last7 = Of(Last30.Where(x => x.Date > today.AddDays(-7)));

        var monthStart = new DateTime(today.Year, today.Month, 1);
        ThisMonth = Of(await _db.VisitDays.AsNoTracking().Where(x => x.Date >= monthStart).ToListAsync());
        var all = await _db.VisitDays.AsNoTracking()
            .GroupBy(x => 1).Select(g => new { V = g.Sum(x => x.PageViews), U = g.Sum(x => x.Visitors), N = g.Sum(x => x.NewVisitors), First = g.Min(x => x.Date) })
            .FirstOrDefaultAsync();
        if (all != null) { AllTime = new(all.V, all.U, all.N); Since = all.First; }

        TopPages = (await _db.VisitPages.AsNoTracking().Where(x => x.Date >= monthStart)
            .GroupBy(x => x.Path).Select(g => new { g.Key, V = g.Sum(x => x.Views) })
            .OrderByDescending(x => x.V).Take(8).ToListAsync())
            .Select(x => (x.Key, x.V)).ToList();
    }

    // "/" → "Home", "/News/Details/3" → "News › Details › 3"
    public static string PageName(string path) =>
        path == "/" ? "Home page" : string.Join(" › ", path.Trim('/').Split('/'));
}
