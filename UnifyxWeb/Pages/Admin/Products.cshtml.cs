using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using UnifyxWeb.Data;

namespace UnifyxWeb.Pages.Admin;

// Products in the "Products" menu: add, change, order, hide and delete.
public class ProductsModel : PageModel
{
    private readonly SiteDbContext _db;
    private readonly ImageStore _images;
    private readonly IMemoryCache _cache;
    public ProductsModel(SiteDbContext db, ImageStore images, IMemoryCache cache) { _db = db; _images = images; _cache = cache; }

    public List<Product> Items { get; set; } = new();
    [BindProperty] public Product Input { get; set; } = new();
    [BindProperty] public IFormFile? Image { get; set; }
    [BindProperty] public bool RemoveImage { get; set; }
    [TempData] public string? Saved { get; set; }

    public async Task OnGetAsync(int? edit)
    {
        Items = await _db.Products.AsNoTracking().OrderBy(x => x.SortOrder).ThenBy(x => x.Id).ToListAsync();
        if (edit.HasValue) Input = Items.FirstOrDefault(x => x.Id == edit) ?? new Product();
        else Input = new Product { SortOrder = Items.Count == 0 ? 1 : Items.Max(x => x.SortOrder) + 1 };
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        string slug = string.IsNullOrWhiteSpace(Input.Slug) ? MakeSlug(Input.NameEn) : Input.Slug.Trim().ToLowerInvariant();
        ModelState.Remove("Input.Slug");
        if (slug.Length == 0 || !Regex.IsMatch(slug, @"^[a-z0-9]+(-[a-z0-9]+)*$"))
            ModelState.AddModelError("Input.Slug", "Page address: use small English letters, numbers and dashes, e.g. unifyx-pos");
        else if (await _db.Products.AnyAsync(x => x.Slug == slug && x.Id != Input.Id))
            ModelState.AddModelError("Input.Slug", "Another product already uses this page address.");

        string? app = WebLink(Input.AppUrl);
        if (!string.IsNullOrWhiteSpace(Input.AppUrl) && app == null)
            ModelState.AddModelError("Input.AppUrl", "The \"Open\" link doesn't look right.");

        string? path = null, error = null;
        if (ModelState.IsValid) (path, error) = await _images.SaveAsync(Image);
        if (error != null) ModelState.AddModelError(nameof(Image), error);
        if (!ModelState.IsValid) { var keep = Input; await OnGetAsync(null); Input = keep; return Page(); }

        var row = Input.Id == 0 ? null : await _db.Products.FindAsync(Input.Id);
        if (row == null) { row = new Product(); _db.Products.Add(row); }
        row.Slug = slug;
        row.NameEn = Input.NameEn.Trim(); row.NameBn = T(Input.NameBn);
        row.Icon = T(Input.Icon);
        row.TaglineEn = T(Input.TaglineEn); row.TaglineBn = T(Input.TaglineBn);
        row.DescriptionEn = T(Input.DescriptionEn); row.DescriptionBn = T(Input.DescriptionBn);
        row.FeaturesEn = T(Input.FeaturesEn); row.FeaturesBn = T(Input.FeaturesBn);
        row.AppUrl = app;
        row.UseHomePage = Input.UseHomePage; row.ComingSoon = Input.ComingSoon;
        row.SortOrder = Input.SortOrder; row.IsActive = Input.IsActive; row.UpdatedAt = DateTime.Now;
        if (path != null) { _images.Delete(row.ImagePath); row.ImagePath = path; }
        else if (RemoveImage) { _images.Delete(row.ImagePath); row.ImagePath = null; }
        await _db.SaveChangesAsync();
        ProductMenu.Clear(_cache);
        Saved = "Saved. The menu shows it now.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var row = await _db.Products.FindAsync(id);
        if (row != null) { _images.Delete(row.ImagePath); _db.Products.Remove(row); await _db.SaveChangesAsync(); ProductMenu.Clear(_cache); Saved = "Deleted."; }
        return RedirectToPage();
    }

    private static string? T(string? s) { s = s?.Trim(); return string.IsNullOrEmpty(s) ? null : s; }

    // "Unifyx POS & Billing" → "unifyx-pos-billing"
    private static string MakeSlug(string? name)
    {
        var s = Regex.Replace((name ?? "").ToLowerInvariant(), @"[^a-z0-9]+", "-").Trim('-');
        return s.Length > 60 ? s[..60].Trim('-') : s;
    }

    private static string? WebLink(string? s)
    {
        s = s?.Trim();
        if (string.IsNullOrEmpty(s)) return null;
        if (!s.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !s.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) s = "https://" + s;
        return Uri.TryCreate(s, UriKind.Absolute, out var u) && (u.Scheme == "https" || u.Scheme == "http") && u.Host.Contains('.') ? u.ToString() : null;
    }
}
