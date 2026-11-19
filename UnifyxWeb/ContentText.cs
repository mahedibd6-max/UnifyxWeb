using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using UnifyxWeb.Data;

namespace UnifyxWeb;

// Website texts: what Admin → Texts saved, otherwise the built-in English / Bangla written in the page.
public class ContentText
{
    public const string CacheKey = "site-texts";
    private readonly SiteDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly Lang _lang;
    private Dictionary<string, (string? En, string? Bn)>? _saved;

    public ContentText(SiteDbContext db, IMemoryCache cache, Lang lang) { _db = db; _cache = cache; _lang = lang; }

    // Text for the current language.
    public string T(string key, string en, string bn)
    {
        var saved = Saved();
        if (saved.TryGetValue(key, out var s))
        {
            if (_lang.IsBangla) { if (!string.IsNullOrWhiteSpace(s.Bn)) return s.Bn!; }
            else if (!string.IsNullOrWhiteSpace(s.En)) return s.En!;
        }
        return _lang.IsBangla ? bn : en;
    }

    public Dictionary<string, (string? En, string? Bn)> Saved()
    {
        if (_saved != null) return _saved;
        _saved = _cache.GetOrCreate(CacheKey, e =>
        {
            e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            try
            {
                return _db.SiteTexts.AsNoTracking().ToList()
                    .ToDictionary(x => x.Key, x => (x.TextEn, x.TextBn));
            }
            catch (Exception) { return new Dictionary<string, (string?, string?)>(); }   // database not ready
        })!;
        return _saved;
    }

    // Call after saving in Admin so the website shows the change at once.
    public static void Clear(IMemoryCache cache) => cache.Remove(CacheKey);
}
