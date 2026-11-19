namespace UnifyxWeb;

public class SiteOptions
{
    public string AppUrl { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string Address { get; set; } = "";
    public string WhatsApp { get; set; } = "";   // number with country code, digits only: 8801XXXXXXXXX
    public string Facebook { get; set; } = "";
    public string LinkedIn { get; set; } = "";
}

// The page's language (from the URL) and small helpers for two-language text and links.
public class Lang
{
    public const string CookieName = "ux-lang";
    private readonly IHttpContextAccessor _http;

    public Lang(IHttpContextAccessor http) => _http = http;

    public string Code =>
        (_http.HttpContext?.GetRouteValue("culture") as string) == "bn" ? "bn" : "en";

    public bool IsBangla => Code == "bn";

    // Text in the current language.
    public string T(string en, string bn) => IsBangla ? bn : en;

    // Link to a page in the current language, e.g. Url("Pricing") → /bn/Pricing
    public string Url(string page = "") => "/" + Code + (string.IsNullOrEmpty(page) ? "" : "/" + page.TrimStart('/'));

    // The same page in the other language.
    public string SwitchUrl()
    {
        var path = _http.HttpContext?.Request.Path.Value ?? "/";
        var other = IsBangla ? "en" : "bn";
        var rest = path.Length > 3 ? path[3..] : "";
        return "/" + other + rest + (_http.HttpContext?.Request.QueryString.Value ?? "");
    }
}
