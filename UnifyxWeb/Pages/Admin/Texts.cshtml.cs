using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using UnifyxWeb.Data;

namespace UnifyxWeb.Pages.Admin;

// Change any website text, page by page.
public class TextsModel : PageModel
{
    private readonly SiteDbContext _db;
    private readonly IMemoryCache _cache;
    public TextsModel(SiteDbContext db, IMemoryCache cache) { _db = db; _cache = cache; }

    public record Row(SiteTextDefault Def, string En, string Bn, bool Changed);
    public record PageTab(string Page, string Name, int Count, int Changed);

    public List<PageTab> Tabs { get; set; } = new();
    public string Tab { get; set; } = "home";
    public string? Find { get; set; }
    public List<Row> Rows { get; set; } = new();
    [TempData] public string? Saved { get; set; }

    public class Item { public string Key { get; set; } = ""; public string? En { get; set; } public string? Bn { get; set; } }
    [BindProperty] public List<Item> Items { get; set; } = new();

    public async Task OnGetAsync(string? tab, string? find)
    {
        var saved = await _db.SiteTexts.AsNoTracking().ToDictionaryAsync(x => x.Key);
        Tabs = SiteTextDefaults.All.GroupBy(x => x.Page)
            .Select(g => new PageTab(g.Key, g.First().PageName, g.Count(), g.Count(d => saved.ContainsKey(d.Key)))).ToList();
        Find = string.IsNullOrWhiteSpace(find) ? null : find.Trim();
        Tab = Tabs.Any(t => t.Page == tab) ? tab! : "home";

        IEnumerable<SiteTextDefault> list = SiteTextDefaults.All;
        if (Find != null)
            list = list.Where(d => Has(d.En, Find) || Has(d.Bn, Find)
                || (saved.TryGetValue(d.Key, out var s) && (Has(s.TextEn, Find) || Has(s.TextBn, Find))));
        else list = list.Where(d => d.Page == Tab);

        Rows = list.Select(d =>
        {
            saved.TryGetValue(d.Key, out var s);
            string en = string.IsNullOrWhiteSpace(s?.TextEn) ? d.En : s!.TextEn!;
            string bn = string.IsNullOrWhiteSpace(s?.TextBn) ? d.Bn : s!.TextBn!;
            return new Row(d, en, bn, s != null);
        }).ToList();
    }

    private static bool Has(string? text, string find) => text != null && text.Contains(find, StringComparison.OrdinalIgnoreCase);

    public async Task<IActionResult> OnPostSaveAsync(string? tab, string? find)
    {
        var rows = await _db.SiteTexts.ToDictionaryAsync(x => x.Key);
        int changed = 0;
        foreach (var item in Items)
        {
            if (!SiteTextDefaults.ByKey.TryGetValue(item.Key, out var def)) continue;
            string? en = Clean(item.En), bn = Clean(item.Bn);
            if (en == def.En) en = null;           // same as built-in: nothing to keep
            if (bn == def.Bn) bn = null;
            rows.TryGetValue(item.Key, out var row);
            if (en == null && bn == null)
            {
                if (row != null) { _db.SiteTexts.Remove(row); changed++; }
                continue;
            }
            if (row == null) { row = new SiteText { Key = item.Key }; _db.SiteTexts.Add(row); }
            if (row.TextEn != en || row.TextBn != bn) { row.TextEn = en; row.TextBn = bn; row.UpdatedAt = DateTime.Now; changed++; }
        }
        await _db.SaveChangesAsync();
        ContentText.Clear(_cache);
        Saved = changed == 0 ? "Nothing changed." : $"Saved {changed} change{(changed == 1 ? "" : "s")}. The website shows them now.";
        return RedirectToPage(new { tab, find });
    }

    public async Task<IActionResult> OnPostResetAsync(string key, string? tab, string? find)
    {
        var row = await _db.SiteTexts.FirstOrDefaultAsync(x => x.Key == key);
        if (row != null) { _db.SiteTexts.Remove(row); await _db.SaveChangesAsync(); ContentText.Clear(_cache); Saved = "Back to the built-in text."; }
        return RedirectToPage(new { tab, find });
    }

    private static string? Clean(string? s)
    {
        s = s?.Trim();
        if (string.IsNullOrEmpty(s)) return null;
        return s.Length > 2000 ? s[..2000] : s;
    }
}
