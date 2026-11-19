using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using UnifyxWeb.Data;

namespace UnifyxWeb;

// Totals shown in the website footer: different visitors and page views since counting began.
// Kept for 5 minutes so the footer does not query the database on every page.
public class VisitStats(SiteDbContext db, IMemoryCache cache, ILogger<VisitStats> log)
{
    public record Totals(int Visitors, int PageViews);

    public async Task<Totals> GetAsync()
    {
        try
        {
            return await cache.GetOrCreateAsync("visit-totals", async e =>
            {
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                int visitors = await db.VisitDays.SumAsync(x => (int?)x.NewVisitors) ?? 0;
                int views = await db.VisitDays.SumAsync(x => (int?)x.PageViews) ?? 0;
                return new Totals(visitors, views);
            }) ?? new Totals(0, 0);
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "Visitor totals could not be read.");
            return new Totals(0, 0);   // never break the page
        }
    }
}
