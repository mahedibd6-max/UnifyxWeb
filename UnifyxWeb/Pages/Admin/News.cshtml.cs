using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UnifyxWeb.Data;

namespace UnifyxWeb.Pages.Admin;

public class NewsModel : PageModel
{
    private readonly SiteDbContext _db;
    private readonly ImageStore _images;
    public NewsModel(SiteDbContext db, ImageStore images) { _db = db; _images = images; }

    public List<NewsPost> Items { get; set; } = new();
    [BindProperty] public NewsPost Input { get; set; } = new();
    [BindProperty] public IFormFile? Image { get; set; }
    [BindProperty] public bool RemoveImage { get; set; }
    [TempData] public string? Saved { get; set; }

    public async Task OnGetAsync(int? edit)
    {
        Items = await _db.NewsPosts.AsNoTracking().OrderByDescending(x => x.PublishedOn).ThenByDescending(x => x.Id).ToListAsync();
        Input = edit.HasValue ? Items.FirstOrDefault(x => x.Id == edit) ?? new NewsPost() : new NewsPost();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        var (path, error) = await _images.SaveAsync(Image);
        if (error != null) ModelState.AddModelError(nameof(Image), error);
        if (!ModelState.IsValid) { var keep = Input; await OnGetAsync(null); Input = keep; return Page(); }

        var row = Input.Id == 0 ? null : await _db.NewsPosts.FindAsync(Input.Id);
        if (row == null) { row = new NewsPost(); _db.NewsPosts.Add(row); }
        row.TitleEn = Input.TitleEn.Trim(); row.TitleBn = Input.TitleBn?.Trim();
        row.BodyEn = Input.BodyEn.Trim(); row.BodyBn = Input.BodyBn?.Trim();
        row.PublishedOn = Input.PublishedOn.Date; row.IsPublished = Input.IsPublished;
        if (path != null) { _images.Delete(row.ImagePath); row.ImagePath = path; }
        else if (RemoveImage) { _images.Delete(row.ImagePath); row.ImagePath = null; }
        await _db.SaveChangesAsync();
        Saved = "Saved.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var row = await _db.NewsPosts.FindAsync(id);
        if (row != null) { _images.Delete(row.ImagePath); _db.NewsPosts.Remove(row); await _db.SaveChangesAsync(); Saved = "Deleted."; }
        return RedirectToPage();
    }
}
