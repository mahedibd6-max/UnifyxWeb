using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UnifyxWeb.Data;

namespace UnifyxWeb.Pages.News;

public class DetailsModel : PageModel
{
    private readonly SiteDbContext _db;
    public DetailsModel(SiteDbContext db) => _db = db;
    public NewsPost Post { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var post = await _db.NewsPosts.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.IsPublished && x.PublishedOn <= DateTime.Today);
        if (post == null) return NotFound();
        Post = post;
        return Page();
    }
}
