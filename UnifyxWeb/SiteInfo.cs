using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UnifyxWeb.Data;

namespace UnifyxWeb;

// Contact details for the pages: what Admin saved, otherwise appsettings.json.
public class SiteInfo
{
    private readonly SiteOptions _defaults;
    private readonly SiteDbContext _db;
    private readonly Lang _lang;
    private SiteOptions? _value;
    public string? HeroImagePath { get; private set; }

    public SiteInfo(IOptions<SiteOptions> defaults, SiteDbContext db, Lang lang)
    {
        _defaults = defaults.Value; _db = db; _lang = lang;
    }

    public SiteOptions Value => _value ??= Load();

    private SiteOptions Load()
    {
        SiteSetting? s = null;
        try { s = _db.SiteSettings.AsNoTracking().OrderBy(x => x.Id).FirstOrDefault(); }
        catch (Exception) { /* database not ready: use defaults */ }
        HeroImagePath = s?.HeroImagePath;
        static string Pick(string? saved, string fallback) => string.IsNullOrWhiteSpace(saved) ? fallback : saved;
        return new SiteOptions
        {
            AppUrl = Pick(s?.LoginUrl, _defaults.AppUrl),
            Phone = Pick(s?.Phone, _defaults.Phone),
            Email = Pick(s?.Email, _defaults.Email),
            Address = Pick(_lang.IsBangla ? (s?.AddressBn ?? s?.AddressEn) : s?.AddressEn, _defaults.Address),
            WhatsApp = Pick(s?.WhatsApp, _defaults.WhatsApp),
            Facebook = Pick(s?.Facebook, _defaults.Facebook),
            LinkedIn = Pick(s?.LinkedIn, _defaults.LinkedIn)
        };
    }
}
