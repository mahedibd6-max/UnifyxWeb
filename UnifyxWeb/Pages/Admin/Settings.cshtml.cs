using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UnifyxWeb.Data;

namespace UnifyxWeb.Pages.Admin;

public class SettingsModel : PageModel
{
    private readonly SiteDbContext _db;
    public SettingsModel(SiteDbContext db) => _db = db;

    [BindProperty] public SiteSetting Input { get; set; } = new();
    [TempData] public string? Saved { get; set; }

    public async Task OnGetAsync()
    {
        Input = await _db.SiteSettings.AsNoTracking().OrderBy(x => x.Id).FirstOrDefaultAsync() ?? new SiteSetting();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Input.Facebook = WebLink(Input.Facebook, "Input.Facebook");
        Input.LinkedIn = WebLink(Input.LinkedIn, "Input.LinkedIn");
        Input.LoginUrl = WebLink(Input.LoginUrl, "Input.LoginUrl", allowLocal: true);
        if (!ModelState.IsValid) return Page();
        var row = await _db.SiteSettings.OrderBy(x => x.Id).FirstOrDefaultAsync();
        if (row == null) { row = new SiteSetting(); _db.SiteSettings.Add(row); }
        row.Phone = Input.Phone?.Trim();
        row.Email = Input.Email?.Trim();
        row.AddressEn = Input.AddressEn?.Trim();
        row.AddressBn = Input.AddressBn?.Trim();
        row.WhatsApp = Input.WhatsApp?.Trim();
        row.Facebook = Input.Facebook?.Trim();
        row.LinkedIn = Input.LinkedIn?.Trim();
        row.LoginUrl = Input.LoginUrl?.Trim();
        row.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        Saved = "Saved. The website now shows the new details.";
        return RedirectToPage();
    }

    // "www.linkedin.com/in/x" → "https://www.linkedin.com/in/x"; only http(s) links are accepted.
    private string? WebLink(string? value, string field, bool allowLocal = false)
    {
        ModelState.Remove(field);
        if (string.IsNullOrWhiteSpace(value)) return null;
        value = value.Trim();
        if (!value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            value = "https://" + value;
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || !(uri.Host.Contains('.') || (allowLocal && uri.Host == "localhost")) || value.Length > 300)
            ModelState.AddModelError(field, "Enter a web address, e.g. www.facebook.com/yourpage");
        return value;
    }
}
