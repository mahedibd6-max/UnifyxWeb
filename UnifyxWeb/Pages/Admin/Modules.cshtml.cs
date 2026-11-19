using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UnifyxWeb.Data;

namespace UnifyxWeb.Pages.Admin;

// Module cards on the home page: add, change, order, hide, delete.
public class ModulesModel : PageModel
{
    private readonly SiteDbContext _db;
    public ModulesModel(SiteDbContext db) => _db = db;

    public List<ModuleItem> Items { get; set; } = new();
    [BindProperty] public ModuleItem Input { get; set; } = new();
    [TempData] public string? Saved { get; set; }

    // The 8 built-in cards: icon and the Texts keys of name, text and three points (home.002 … home.041).
    private static readonly string[] BuiltInIcons = { "👥", "💰", "📦", "🛒", "🧾", "📒", "🏢", "🎫" };

    public async Task OnGetAsync(int? edit)
    {
        Items = await _db.ModuleItems.AsNoTracking().OrderBy(x => x.SortOrder).ThenBy(x => x.Id).ToListAsync();
        if (edit.HasValue) Input = Items.FirstOrDefault(x => x.Id == edit) ?? new ModuleItem();
        else Input = new ModuleItem { SortOrder = Items.Count == 0 ? 1 : Items.Max(x => x.SortOrder) + 1 };
    }

    // Copies the 8 built-in cards (with any changes made in Texts) so they can be edited here.
    public async Task<IActionResult> OnPostImportAsync()
    {
        if (await _db.ModuleItems.AnyAsync()) { Saved = "The list already has modules."; return RedirectToPage(); }
        var saved = await _db.SiteTexts.AsNoTracking().ToDictionaryAsync(x => x.Key);
        (string En, string Bn) Get(int n)
        {
            string key = $"home.{n:000}";
            var d = SiteTextDefaults.ByKey[key];
            saved.TryGetValue(key, out var s);
            return (string.IsNullOrWhiteSpace(s?.TextEn) ? d.En : s!.TextEn!, string.IsNullOrWhiteSpace(s?.TextBn) ? d.Bn : s!.TextBn!);
        }
        for (int i = 0; i < 8; i++)
        {
            int k = 2 + i * 5;
            var name = Get(k); var text = Get(k + 1); var p1 = Get(k + 2); var p2 = Get(k + 3); var p3 = Get(k + 4);
            _db.ModuleItems.Add(new ModuleItem
            {
                Icon = BuiltInIcons[i], NameEn = name.En, NameBn = name.Bn, TextEn = text.En, TextBn = text.Bn,
                PointsEn = string.Join("\n", p1.En, p2.En, p3.En), PointsBn = string.Join("\n", p1.Bn, p2.Bn, p3.Bn),
                SortOrder = i + 1, IsActive = true
            });
        }
        await _db.SaveChangesAsync();
        Saved = "The 8 modules are now in the list. Change them or add new ones below.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (!ModelState.IsValid) { var keep = Input; await OnGetAsync(null); Input = keep; return Page(); }
        var row = Input.Id == 0 ? null : await _db.ModuleItems.FindAsync(Input.Id);
        if (row == null) { row = new ModuleItem(); _db.ModuleItems.Add(row); }
        row.Icon = T(Input.Icon); row.NameEn = Input.NameEn.Trim(); row.NameBn = T(Input.NameBn);
        row.TextEn = T(Input.TextEn); row.TextBn = T(Input.TextBn);
        row.PointsEn = T(Input.PointsEn); row.PointsBn = T(Input.PointsBn);
        row.SortOrder = Input.SortOrder; row.IsActive = Input.IsActive;
        await _db.SaveChangesAsync();
        Saved = "Saved.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var row = await _db.ModuleItems.FindAsync(id);
        if (row != null) { _db.ModuleItems.Remove(row); await _db.SaveChangesAsync(); Saved = "Deleted."; }
        return RedirectToPage();
    }

    private static string? T(string? s) { s = s?.Trim(); return string.IsNullOrEmpty(s) ? null : s; }
}
