using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UnifyxWeb.Data;

namespace UnifyxWeb.Pages.Admin;

// Demo requests sent from the website's Contact page.
public class DemoRequestsModel : PageModel
{
    private readonly SiteDbContext _db;
    private readonly DemoMailer _mailer;
    public DemoRequestsModel(SiteDbContext db, DemoMailer mailer) { _db = db; _mailer = mailer; }

    public List<DemoRequest> Items { get; set; } = new();
    public string Show { get; set; } = "new";
    public int NewCount { get; set; }
    public int AllCount { get; set; }
    public int? Open { get; set; }
    public bool MailSetUp => _mailer.IsSetUp;
    [TempData] public string? Saved { get; set; }

    public async Task OnGetAsync(string? show, int? open)
    {
        Show = show == "all" ? "all" : "new";
        Open = open;
        NewCount = await _db.DemoRequests.CountAsync(x => !x.Contacted);
        AllCount = await _db.DemoRequests.CountAsync();
        var q = _db.DemoRequests.AsNoTracking();
        if (Show == "new" && open == null) q = q.Where(x => !x.Contacted);
        Items = await q.OrderByDescending(x => x.CreatedAt).Take(300).ToListAsync();
    }

    public async Task<IActionResult> OnPostContactedAsync(int id, bool value, string? show)
    {
        var row = await _db.DemoRequests.FindAsync(id);
        if (row != null) { row.Contacted = value; await _db.SaveChangesAsync(); Saved = value ? "Marked as contacted." : "Moved back to new."; }
        return RedirectToPage(new { show });
    }

    public async Task<IActionResult> OnPostNoteAsync(int id, string? note, string? show)
    {
        var row = await _db.DemoRequests.FindAsync(id);
        if (row != null)
        {
            note = note?.Trim();
            row.AdminNote = string.IsNullOrEmpty(note) ? null : (note.Length > 1000 ? note[..1000] : note);
            await _db.SaveChangesAsync(); Saved = "Note saved.";
        }
        return RedirectToPage(new { show });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, string? show)
    {
        var row = await _db.DemoRequests.FindAsync(id);
        if (row != null) { _db.DemoRequests.Remove(row); await _db.SaveChangesAsync(); Saved = "Deleted."; }
        return RedirectToPage(new { show });
    }

    // 01712345678 / +880 1712-345678 → 8801712345678 for a WhatsApp link.
    public static string WhatsAppNumber(string phone)
    {
        var d = new string(phone.Where(char.IsDigit).ToArray());
        if (d.StartsWith("01") && d.Length == 11) d = "88" + d;
        return d;
    }
}
