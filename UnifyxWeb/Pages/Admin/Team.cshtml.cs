using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UnifyxWeb.Data;

namespace UnifyxWeb.Pages.Admin;

// "Our team" on the home page: add, change, order, hide and delete people.
public class TeamModel : PageModel
{
    private readonly SiteDbContext _db;
    private readonly ImageStore _images;
    public TeamModel(SiteDbContext db, ImageStore images) { _db = db; _images = images; }

    public List<TeamMember> Items { get; set; } = new();
    [BindProperty] public TeamMember Input { get; set; } = new();
    [BindProperty] public IFormFile? Image { get; set; }
    [BindProperty] public bool RemoveImage { get; set; }
    [TempData] public string? Saved { get; set; }

    public async Task OnGetAsync(int? edit)
    {
        Items = await _db.TeamMembers.AsNoTracking().OrderBy(x => x.SortOrder).ThenBy(x => x.Id).ToListAsync();
        if (edit.HasValue) Input = Items.FirstOrDefault(x => x.Id == edit) ?? new TeamMember();
        else Input = new TeamMember { SortOrder = Items.Count == 0 ? 1 : Items.Max(x => x.SortOrder) + 1 };
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        string? link = WebLink(Input.LinkedIn);
        if (!string.IsNullOrWhiteSpace(Input.LinkedIn) && link == null)
            ModelState.AddModelError("Input.LinkedIn", "The LinkedIn link doesn't look right.");
        string? path = null, error = null;
        if (ModelState.IsValid) (path, error) = await _images.SaveAsync(Image);
        if (error != null) ModelState.AddModelError(nameof(Image), error);
        if (!ModelState.IsValid) { var keep = Input; await OnGetAsync(null); Input = keep; return Page(); }

        var row = Input.Id == 0 ? null : await _db.TeamMembers.FindAsync(Input.Id);
        if (row == null) { row = new TeamMember(); _db.TeamMembers.Add(row); }
        row.NameEn = Input.NameEn.Trim(); row.NameBn = Input.NameBn?.Trim();
        row.RoleEn = Input.RoleEn.Trim(); row.RoleBn = Input.RoleBn?.Trim();
        row.BioEn = Input.BioEn?.Trim(); row.BioBn = Input.BioBn?.Trim();
        row.LinkedIn = link;
        row.SortOrder = Input.SortOrder; row.IsActive = Input.IsActive;
        if (path != null) { _images.Delete(row.ImagePath); row.ImagePath = path; }
        else if (RemoveImage) { _images.Delete(row.ImagePath); row.ImagePath = null; }
        await _db.SaveChangesAsync();
        Saved = "Saved.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var row = await _db.TeamMembers.FindAsync(id);
        if (row != null) { _images.Delete(row.ImagePath); _db.TeamMembers.Remove(row); await _db.SaveChangesAsync(); Saved = "Deleted."; }
        return RedirectToPage();
    }

    // "linkedin.com/in/x" → "https://linkedin.com/in/x"; only http(s) links are kept.
    private static string? WebLink(string? s)
    {
        s = s?.Trim();
        if (string.IsNullOrEmpty(s)) return null;
        if (!s.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !s.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            s = "https://" + s;
        return Uri.TryCreate(s, UriKind.Absolute, out var u) && (u.Scheme == "https" || u.Scheme == "http") && u.Host.Contains('.')
            ? u.ToString() : null;
    }
}
