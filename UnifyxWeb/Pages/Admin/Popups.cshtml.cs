using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UnifyxWeb.Data;

namespace UnifyxWeb.Pages.Admin;

public class PopupsModel : PageModel
{
    private readonly SiteDbContext _db;
    private readonly ImageStore _images;
    public PopupsModel(SiteDbContext db, ImageStore images) { _db = db; _images = images; }

    public List<Popup> Items { get; set; } = new();
    [BindProperty] public Popup Input { get; set; } = new();
    [BindProperty] public IFormFile? Image { get; set; }
    [BindProperty] public bool RemoveImage { get; set; }
    [TempData] public string? Saved { get; set; }

    public async Task OnGetAsync(int? edit)
    {
        Items = await _db.Popups.AsNoTracking().OrderByDescending(x => x.UpdatedAt).ToListAsync();
        Input = edit.HasValue ? Items.FirstOrDefault(x => x.Id == edit) ?? new Popup() : new Popup();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (Input.EndDate < Input.StartDate) ModelState.AddModelError("Input.EndDate", "The end date is before the start date.");
        if (!string.IsNullOrWhiteSpace(Input.LinkUrl) && !(Input.LinkUrl.StartsWith("/") || Input.LinkUrl.StartsWith("https://") || Input.LinkUrl.StartsWith("http://")))
            Input.LinkUrl = "https://" + Input.LinkUrl.Trim();
        var (path, error) = await _images.SaveAsync(Image);
        if (error != null) ModelState.AddModelError(nameof(Image), error);
        if (!ModelState.IsValid) { var keep = Input; await OnGetAsync(null); Input = keep; return Page(); }

        var row = Input.Id == 0 ? null : await _db.Popups.FindAsync(Input.Id);
        if (row == null) { row = new Popup(); _db.Popups.Add(row); }
        row.TitleEn = Input.TitleEn.Trim(); row.TitleBn = Input.TitleBn?.Trim();
        row.TextEn = Input.TextEn?.Trim(); row.TextBn = Input.TextBn?.Trim();
        row.LinkUrl = Input.LinkUrl?.Trim(); row.LinkTextEn = Input.LinkTextEn?.Trim(); row.LinkTextBn = Input.LinkTextBn?.Trim();
        row.StartDate = Input.StartDate.Date; row.EndDate = Input.EndDate.Date; row.IsActive = Input.IsActive;
        if (path != null) { _images.Delete(row.ImagePath); row.ImagePath = path; }
        else if (RemoveImage) { _images.Delete(row.ImagePath); row.ImagePath = null; }
        row.UpdatedAt = DateTime.Now;   // visitors who closed the old version see the changed one again
        await _db.SaveChangesAsync();
        Saved = "Saved.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var row = await _db.Popups.FindAsync(id);
        if (row != null) { _images.Delete(row.ImagePath); _db.Popups.Remove(row); await _db.SaveChangesAsync(); Saved = "Deleted."; }
        return RedirectToPage();
    }
}
