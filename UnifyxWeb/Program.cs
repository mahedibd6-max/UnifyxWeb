using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UnifyxWeb;
using UnifyxWeb.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages(options =>
{
    // Public pages live under /en/... or /bn/...; admin pages under /admin and need a login.
    options.Conventions.Add(new CultureRouteConvention());
    options.Conventions.AuthorizeFolder("/Admin");
    options.Conventions.AllowAnonymousToPage("/Admin/Login");
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Lang>();
builder.Services.AddScoped<SiteInfo>();
builder.Services.AddScoped<ImageStore>();
builder.Services.AddScoped<PopupService>();
builder.Services.AddScoped<DemoMailer>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ContentText>();
builder.Services.AddScoped<ProductMenu>();
builder.Services.AddScoped<VisitStats>();
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.Configure<SiteOptions>(builder.Configuration.GetSection("Site"));

builder.Services.AddDbContext<SiteDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("SiteConnection")));

builder.Services.AddIdentityCore<IdentityUser>(o =>
    {
        o.Password.RequiredLength = 8;
        o.Lockout.MaxFailedAccessAttempts = 5;
        o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        o.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<SiteDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();
builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath = "/admin/login";
    o.AccessDeniedPath = "/admin/login";
    o.Cookie.Name = "ux-admin";
    o.ExpireTimeSpan = TimeSpan.FromHours(8);
    o.SlidingExpiration = true;
});

// Limits per address: 10 login tries a minute; 5 demo requests every 10 minutes.
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.OnRejected = async (ctx, token) =>
    {
        ctx.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
        await ctx.HttpContext.Response.WriteAsync(
            "Too many tries. Please wait a few minutes and try again. / অনেকবার চেষ্টা হয়েছে, কয়েক মিনিট পরে আবার চেষ্টা করুন।", token);
    };
    o.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
    {
        bool post = HttpMethods.IsPost(ctx.Request.Method);
        string path = ctx.Request.Path.Value ?? "";
        bool login = post && path.StartsWith("/admin/login", StringComparison.OrdinalIgnoreCase);
        bool demo = post && path.EndsWith("/contact", StringComparison.OrdinalIgnoreCase);
        if (!login && !demo) return RateLimitPartition.GetNoLimiter("open");
        string ip = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        if (ctx.Connection.RemoteIpAddress is { } r && System.Net.IPAddress.IsLoopback(r)
            && ctx.Request.Headers.TryGetValue("CF-Connecting-IP", out var cf) && !string.IsNullOrWhiteSpace(cf))
            ip = cf.ToString();
        return login
            ? RateLimitPartition.GetFixedWindowLimiter("login:" + ip, _ => new FixedWindowRateLimiterOptions
                { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 })
            : RateLimitPartition.GetFixedWindowLimiter("demo:" + ip, _ => new FixedWindowRateLimiterOptions
                { PermitLimit = 5, Window = TimeSpan.FromMinutes(10), QueueLimit = 0 });
    });
});

var app = builder.Build();

await SiteSeed.RunAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/en/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Security headers on every response.
app.Use(async (context, next) =>
{
    var h = context.Response.Headers;
    h["X-Content-Type-Options"] = "nosniff";
    h["X-Frame-Options"] = "SAMEORIGIN";
    h["Referrer-Policy"] = "strict-origin-when-cross-origin";
    h["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    await next();
});

app.UseStaticFiles();
app.UseRouting();
app.UseMiddleware<VisitCounter>();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// "/" → the visitor's language: remembered choice, else the browser's, else English.
app.MapGet("/", (HttpContext ctx) =>
{
    string lang = ctx.Request.Cookies[Lang.CookieName]
        ?? (ctx.Request.Headers.AcceptLanguage.ToString().StartsWith("bn", StringComparison.OrdinalIgnoreCase) ? "bn" : "en");
    return Results.Redirect(lang == "bn" ? "/bn" : "/en");
});

app.MapSeo();          // /robots.txt and /sitemap.xml
app.MapRazorPages();

app.Run();
