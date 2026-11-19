using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UnifyxWeb.Data;

namespace UnifyxWeb.Pages.Admin;

public class HomeModel : PageModel
{
    private readonly SiteDbContext _db;
    private readonly ImageStore _images;
    public HomeModel(SiteDbContext db, ImageStore images) { _db = db; _images = images; }

    [BindProperty] public HomeContent Input { get; set; } = new();
    [BindProperty] public List<HomeStat> Stats { get; set; } = new();
    [BindProperty] public IFormFile? HeroImage { get; set; }
    [BindProperty] public bool RemoveImage { get; set; }
    [TempData] public string? Saved { get; set; }

    public async Task OnGetAsync()
    {
        Input = await _db.HomeContents.AsNoTracking().OrderBy(x => x.Id).FirstOrDefaultAsync() ?? new HomeContent();
        Stats = await _db.HomeStats.AsNoTracking().OrderBy(x => x.SortOrder).ThenBy(x => x.Id).Take(4).ToListAsync();
        while (Stats.Count < 4) Stats.Add(new HomeStat { SortOrder = Stats.Count });
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // A stat row with an empty value is simply not shown, so it is not an error.
        for (int i = 0; i < Stats.Count; i++)
            if (string.IsNullOrWhiteSpace(Stats[i].Value) && string.IsNullOrWhiteSpace(Stats[i].LabelEn))
            { ModelState.Remove($"Stats[{i}].Value"); ModelState.Remove($"Stats[{i}].LabelEn"); }

        var (path, error) = await _images.SaveAsync(HeroImage);
        if (error != null) ModelState.AddModelError(nameof(HeroImage), error);
        if (!ModelState.IsValid) return Page();

        var row = await _db.HomeContents.OrderBy(x => x.Id).FirstOrDefaultAsync();
        if (row == null) { row = new HomeContent(); _db.HomeContents.Add(row); }
        row.EyebrowEn = Input.EyebrowEn?.Trim(); row.EyebrowBn = Input.EyebrowBn?.Trim();
        row.TitleEn = Input.TitleEn?.Trim(); row.TitleBn = Input.TitleBn?.Trim();
        row.LeadEn = Input.LeadEn?.Trim(); row.LeadBn = Input.LeadBn?.Trim();
        if (path != null) { _images.Delete(row.HeroImagePath); row.HeroImagePath = path; }
        else if (RemoveImage) { _images.Delete(row.HeroImagePath); row.HeroImagePath = null; }
        row.UpdatedAt = DateTime.Now;

        _db.HomeStats.RemoveRange(_db.HomeStats);
        int order = 0;
        foreach (var s in Stats.Where(s => !string.IsNullOrWhiteSpace(s.Value) && !string.IsNullOrWhiteSpace(s.LabelEn)).Take(4))
            _db.HomeStats.Add(new HomeStat { Value = s.Value.Trim(), LabelEn = s.LabelEn.Trim(), LabelBn = s.LabelBn?.Trim(), SortOrder = order++ });

        await _db.SaveChangesAsync();
        Saved = "Saved. Open the website to see the change.";
        return RedirectToPage();
    }
}
