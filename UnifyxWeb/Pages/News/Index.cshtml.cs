using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UnifyxWeb.Data;

namespace UnifyxWeb.Pages.News;

public class IndexModel : PageModel
{
    private readonly SiteDbContext _db;
    public IndexModel(SiteDbContext db) => _db = db;
    public List<NewsPost> Items { get; set; } = new();

    public async Task OnGetAsync()
    {
        try
        {
            Items = await _db.NewsPosts.AsNoTracking().Where(x => x.IsPublished && x.PublishedOn <= DateTime.Today)
                .OrderByDescending(x => x.PublishedOn).ThenByDescending(x => x.Id).Take(60).ToListAsync();
        }
        catch (Exception) { }
    }
}
