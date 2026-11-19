using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UnifyxWeb.Data;

namespace UnifyxWeb.Pages.Admin;

// List, add, change and delete questions on one page.
public class FaqsModel : PageModel
{
    private readonly SiteDbContext _db;
    public FaqsModel(SiteDbContext db) => _db = db;

    public List<FaqItem> Items { get; set; } = new();
    [BindProperty] public FaqItem Input { get; set; } = new();
    [TempData] public string? Saved { get; set; }

    public async Task OnGetAsync(int? edit)
    {
        Items = await _db.FaqItems.AsNoTracking().OrderBy(x => x.SortOrder).ThenBy(x => x.Id).ToListAsync();
        if (edit.HasValue) Input = Items.FirstOrDefault(x => x.Id == edit) ?? new FaqItem();
        else Input = new FaqItem { SortOrder = Items.Count == 0 ? 1 : Items.Max(x => x.SortOrder) + 1 };
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (!ModelState.IsValid) { await OnGetAsync(null); return Page(); }
        var row = Input.Id == 0 ? null : await _db.FaqItems.FindAsync(Input.Id);
        if (row == null) { row = new FaqItem(); _db.FaqItems.Add(row); }
        row.QuestionEn = Input.QuestionEn.Trim(); row.QuestionBn = Input.QuestionBn?.Trim();
        row.AnswerEn = Input.AnswerEn.Trim(); row.AnswerBn = Input.AnswerBn?.Trim();
        row.SortOrder = Input.SortOrder; row.IsActive = Input.IsActive;
        await _db.SaveChangesAsync();
        Saved = "Saved.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var row = await _db.FaqItems.FindAsync(id);
        if (row != null) { _db.FaqItems.Remove(row); await _db.SaveChangesAsync(); Saved = "Deleted."; }
        return RedirectToPage();
    }
}
