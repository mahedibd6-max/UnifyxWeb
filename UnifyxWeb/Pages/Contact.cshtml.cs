using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UnifyxWeb.Data;

namespace UnifyxWeb.Pages;

// Contact page with the "Request a demo" form.
public class ContactModel : PageModel
{
    private readonly SiteDbContext _db;
    private readonly DemoMailer _mailer;
    private readonly Lang _lang;
    private readonly IDataProtector _stamp;

    public ContactModel(SiteDbContext db, DemoMailer mailer, Lang lang, IDataProtectionProvider dp)
    {
        _db = db; _mailer = mailer; _lang = lang;
        _stamp = dp.CreateProtector("UnifyxWeb.DemoForm.v1");
    }

    // Choices shown on the form: (key saved, English, Bangla).
    public static readonly (string Key, string En, string Bn)[] ModuleChoices =
    {
        ("HRM", "HR & Attendance", "এইচআর ও হাজিরা"),
        ("Payroll", "Payroll & tax", "বেতন ও আয়কর"),
        ("Inventory", "Inventory", "স্টক"),
        ("Purchase", "Purchase", "ক্রয়"),
        ("Sales", "Sales", "বিক্রয়"),
        ("Accounts", "Accounts", "হিসাব"),
        ("Asset", "Assets", "সম্পদ"),
        ("IssueTracking", "Issue Tracking", "কাজের টিকেট")
    };
    public static readonly (string Key, string En, string Bn)[] EmployeeChoices =
    {
        ("1-25", "1 – 25", "১ – ২৫"),
        ("26-100", "26 – 100", "২৬ – ১০০"),
        ("101-300", "101 – 300", "১০১ – ৩০০"),
        ("300+", "More than 300", "৩০০-এর বেশি")
    };

    public class DemoInput
    {
        [Required(ErrorMessage = "name"), StringLength(120)] public string Name { get; set; } = "";
        [Required(ErrorMessage = "company"), StringLength(160)] public string Company { get; set; } = "";
        [Required(ErrorMessage = "phone"), RegularExpression(@"^[0-9+\-\s()]{8,20}$", ErrorMessage = "phone")]
        public string Phone { get; set; } = "";
        [EmailAddress(ErrorMessage = "email"), StringLength(120)] public string? Email { get; set; }
        [StringLength(30)] public string? Employees { get; set; }
        public List<string> Modules { get; set; } = new();
        [StringLength(1000)] public string? Message { get; set; }
    }

    [BindProperty] public DemoInput Input { get; set; } = new();
    // Left empty by people; filled by many spam bots (hidden from view).
    [BindProperty] public string? Website { get; set; }
    [BindProperty] public string? Stamp { get; set; }

    public bool Sent { get; set; }

    public void OnGet(int? sent)
    {
        Sent = sent == 1;
        Stamp = _stamp.Protect(DateTime.UtcNow.Ticks.ToString());
        // Modules picked on the Pricing page arrive as ?m=HRM,Payroll — tick them on the form.
        var picked = Request.Query["m"].ToString().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var allowed = ModuleChoices.Select(m => m.Key).ToHashSet();
        Input.Modules = picked.Where(allowed.Contains).Distinct().ToList();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Bots: pretend it worked, save nothing.
        if (!string.IsNullOrEmpty(Website) || !LooksHuman(Stamp))
            return Redirect(_lang.Url("Contact") + "?sent=1#demo");

        if (!ModelState.IsValid)
        {
            Stamp = _stamp.Protect(DateTime.UtcNow.AddSeconds(-10).Ticks.ToString());
            return Page();
        }

        var allowed = ModuleChoices.Select(m => m.Key).ToHashSet();
        var employees = EmployeeChoices.Any(e => e.Key == Input.Employees) ? Input.Employees : null;
        var row = new DemoRequest
        {
            Name = Input.Name.Trim(),
            Company = Input.Company.Trim(),
            Phone = Input.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(Input.Email) ? null : Input.Email.Trim(),
            Employees = employees,
            Modules = string.Join(",", Input.Modules.Where(allowed.Contains).Distinct()),
            Message = string.IsNullOrWhiteSpace(Input.Message) ? null : Input.Message.Trim(),
            Language = _lang.Code,
            CreatedAt = DateTime.Now
        };
        _db.DemoRequests.Add(row);
        await _db.SaveChangesAsync();

        string adminLink = $"{Request.Scheme}://{Request.Host}/admin/demorequests?open={row.Id}";
        await _mailer.NotifyAsync(row, adminLink);

        return Redirect(_lang.Url("Contact") + "?sent=1#demo");
    }

    // The form must have been opened on this site at least 3 seconds and at most 1 day ago.
    private bool LooksHuman(string? stamp)
    {
        if (string.IsNullOrEmpty(stamp)) return false;
        try
        {
            var opened = new DateTime(long.Parse(_stamp.Unprotect(stamp)), DateTimeKind.Utc);
            var age = DateTime.UtcNow - opened;
            return age >= TimeSpan.FromSeconds(3) && age <= TimeSpan.FromDays(1);
        }
        catch { return false; }
    }

    public bool HasError(string key) => ModelState.TryGetValue("Input." + key, out var e) && e.Errors.Count > 0;
}
