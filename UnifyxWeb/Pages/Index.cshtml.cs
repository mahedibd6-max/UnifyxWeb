using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UnifyxWeb.Data;

namespace UnifyxWeb.Pages;

// Home page: what Admin saved, otherwise the built-in text in Index.cshtml.
public class IndexModel : PageModel
{
    private readonly SiteDbContext _db;
    public IndexModel(SiteDbContext db) => _db = db;

    public HomeContent? Home { get; set; }
    public List<HomeStat> Stats { get; set; } = new();
    public List<FaqItem> Faqs { get; set; } = new();
    public List<NewsPost> News { get; set; } = new();
    public List<TeamMember> Team { get; set; } = new();
    public List<ModuleItem> Modules { get; set; } = new();

    public async Task OnGetAsync()
    {
        try
        {
            Home = await _db.HomeContents.AsNoTracking().OrderBy(x => x.Id).FirstOrDefaultAsync();
            Stats = await _db.HomeStats.AsNoTracking().OrderBy(x => x.SortOrder).ThenBy(x => x.Id).Take(4).ToListAsync();
            Faqs = await _db.FaqItems.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.SortOrder).ThenBy(x => x.Id).ToListAsync();
            News = await _db.NewsPosts.AsNoTracking().Where(x => x.IsPublished && x.PublishedOn <= DateTime.Today)
                .OrderByDescending(x => x.PublishedOn).ThenByDescending(x => x.Id).Take(3).ToListAsync();
            Modules = await _db.ModuleItems.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.SortOrder).ThenBy(x => x.Id).ToListAsync();
            Team = await _db.TeamMembers.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.SortOrder).ThenBy(x => x.Id).ToListAsync();
        }
        catch (Exception) { /* database not ready: built-in text is shown */ }
    }
}
