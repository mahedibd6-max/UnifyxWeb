using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using UnifyxWeb.Data;

namespace UnifyxWeb;

// The products shown in the menu (kept in memory; Admin clears it after a change).
public class ProductMenu
{
    public const string CacheKey = "product-menu";
    private readonly SiteDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly Lang _lang;
    private List<Product>? _items;

    public ProductMenu(SiteDbContext db, IMemoryCache cache, Lang lang) { _db = db; _cache = cache; _lang = lang; }

    public List<Product> Items => _items ??= _cache.GetOrCreate(CacheKey, e =>
    {
        e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
        try
        {
            return _db.Products.AsNoTracking().Where(x => x.IsActive)
                .OrderBy(x => x.SortOrder).ThenBy(x => x.Id).ToList();
        }
        catch (Exception) { return new List<Product>(); }   // database not ready
    })!;

    public string Name(Product p) => Pick(p.NameEn, p.NameBn);
    public string Tagline(Product p) => Pick(p.TaglineEn, p.TaglineBn);
    public string Description(Product p) => Pick(p.DescriptionEn, p.DescriptionBn);
    public string[] Features(Product p) =>
        Pick(p.FeaturesEn, p.FeaturesBn).Replace("\r", "").Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    // Main product → the home page modules; others → their own page.
    public string Link(Product p) => p.UseHomePage ? _lang.Url() + "#modules" : _lang.Url("Products/" + p.Slug);

    private string Pick(string? en, string? bn) =>
        (_lang.IsBangla && !string.IsNullOrWhiteSpace(bn) ? bn : en) ?? "";

    public static void Clear(IMemoryCache cache) => cache.Remove(CacheKey);
}
