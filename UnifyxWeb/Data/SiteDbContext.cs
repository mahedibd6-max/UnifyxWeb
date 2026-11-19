using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace UnifyxWeb.Data;

// The website's own small database (separate from the ERP).
public class SiteDbContext : IdentityDbContext<IdentityUser>
{
    public SiteDbContext(DbContextOptions<SiteDbContext> options) : base(options) { }

    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();
    public DbSet<NewsPost> NewsPosts => Set<NewsPost>();
    public DbSet<Popup> Popups => Set<Popup>();
    public DbSet<DemoRequest> DemoRequests => Set<DemoRequest>();
    public DbSet<HomeContent> HomeContents => Set<HomeContent>();
    public DbSet<HomeStat> HomeStats => Set<HomeStat>();
    public DbSet<FaqItem> FaqItems => Set<FaqItem>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<SiteText> SiteTexts => Set<SiteText>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ModuleItem> ModuleItems => Set<ModuleItem>();
    public DbSet<VisitDay> VisitDays => Set<VisitDay>();
    public DbSet<VisitPage> VisitPages => Set<VisitPage>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<SiteText>().HasIndex(x => x.Key).IsUnique();
        b.Entity<Product>().HasIndex(x => x.Slug).IsUnique();
        b.Entity<VisitDay>().HasIndex(x => x.Date).IsUnique();
        b.Entity<VisitPage>().HasIndex(x => new { x.Date, x.Path }).IsUnique();
    }
}

// One row: the top of the home page. Empty boxes fall back to the built-in text.
public class HomeContent
{
    public int Id { get; set; }
    [StringLength(80)] public string? EyebrowEn { get; set; }
    [StringLength(80)] public string? EyebrowBn { get; set; }
    [StringLength(160)] public string? TitleEn { get; set; }
    [StringLength(160)] public string? TitleBn { get; set; }
    [StringLength(600)] public string? LeadEn { get; set; }
    [StringLength(600)] public string? LeadBn { get; set; }
    [StringLength(300)] public string? HeroImagePath { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

// The four numbers under the top of the home page.
public class HomeStat
{
    public int Id { get; set; }
    [Required, StringLength(20)] public string Value { get; set; } = "";
    [Required, StringLength(80)] public string LabelEn { get; set; } = "";
    [StringLength(80)] public string? LabelBn { get; set; }
    public int SortOrder { get; set; }
}

public class FaqItem
{
    public int Id { get; set; }
    [Required, StringLength(200)] public string QuestionEn { get; set; } = "";
    [StringLength(200)] public string? QuestionBn { get; set; }
    [Required, StringLength(2000)] public string AnswerEn { get; set; } = "";
    [StringLength(2000)] public string? AnswerBn { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

// Website visits, one row per day (no personal data is kept).
public class VisitDay
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int PageViews { get; set; }      // every page opened
    public int Visitors { get; set; }       // different browsers that day
    public int NewVisitors { get; set; }    // browsers seen for the first time
}

// Page views per page per day, for "most visited pages".
public class VisitPage
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    [Required, StringLength(200)] public string Path { get; set; } = "";
    public int Views { get; set; }
}

// A module card in "Modules" on the home page. While none is saved, the built-in 8 show.
public class ModuleItem
{
    public int Id { get; set; }
    [StringLength(8)] public string? Icon { get; set; }
    [Required, StringLength(80)] public string NameEn { get; set; } = "";
    [StringLength(80)] public string? NameBn { get; set; }
    [StringLength(300)] public string? TextEn { get; set; }
    [StringLength(300)] public string? TextBn { get; set; }
    [StringLength(800)] public string? PointsEn { get; set; }   // one per line
    [StringLength(800)] public string? PointsBn { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

// A product shown under "Products" in the menu, with its own page.
public class Product
{
    public int Id { get; set; }
    // The page address: /en/Products/<slug>. Small letters, numbers and dashes.
    [StringLength(60), RegularExpression(@"^[a-z0-9]+(-[a-z0-9]+)*$", ErrorMessage = "Use small letters, numbers and dashes only, e.g. unifyx-pos")]
    public string Slug { get; set; } = "";
    [Required, StringLength(80)] public string NameEn { get; set; } = "";
    [StringLength(80)] public string? NameBn { get; set; }
    [StringLength(8)] public string? Icon { get; set; }                 // an emoji, e.g. 🧾
    [StringLength(160)] public string? TaglineEn { get; set; }
    [StringLength(160)] public string? TaglineBn { get; set; }
    [StringLength(4000)] public string? DescriptionEn { get; set; }
    [StringLength(4000)] public string? DescriptionBn { get; set; }
    [StringLength(2000)] public string? FeaturesEn { get; set; }        // one per line
    [StringLength(2000)] public string? FeaturesBn { get; set; }
    [StringLength(300)] public string? ImagePath { get; set; }
    [StringLength(300)] public string? AppUrl { get; set; }             // "Open" button, e.g. https://app.unifyxerp.com
    public bool UseHomePage { get; set; }                               // main product: menu goes to the home page
    public bool ComingSoon { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

// A website text changed in Admin → Texts. Only changed texts have a row; the rest use the built-in text.
public class SiteText
{
    public int Id { get; set; }
    [Required, StringLength(60)] public string Key { get; set; } = "";
    [StringLength(2000)] public string? TextEn { get; set; }
    [StringLength(2000)] public string? TextBn { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

// People shown in "Our team" on the home page.
public class TeamMember
{
    public int Id { get; set; }
    [Required, StringLength(120)] public string NameEn { get; set; } = "";
    [StringLength(120)] public string? NameBn { get; set; }
    [Required, StringLength(120)] public string RoleEn { get; set; } = "";
    [StringLength(120)] public string? RoleBn { get; set; }
    [StringLength(300)] public string? BioEn { get; set; }
    [StringLength(300)] public string? BioBn { get; set; }
    [StringLength(300)] public string? ImagePath { get; set; }
    [StringLength(300)] public string? LinkedIn { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

// One row: contact details and links shown across the site.
public class SiteSetting
{
    public int Id { get; set; }
    [StringLength(40)] public string? Phone { get; set; }
    [StringLength(120), EmailAddress] public string? Email { get; set; }
    [StringLength(200)] public string? AddressEn { get; set; }
    [StringLength(200)] public string? AddressBn { get; set; }
    [StringLength(20), RegularExpression(@"^\d{8,15}$", ErrorMessage = "Digits only, with country code, e.g. 8801712345678")]
    public string? WhatsApp { get; set; }
    // Links may be typed without https:// — the admin page adds it.
    [StringLength(300)] public string? Facebook { get; set; }
    [StringLength(300)] public string? LinkedIn { get; set; }
    [StringLength(300)] public string? HeroImagePath { get; set; }
    // Where the "Login" button goes (the ERP). Empty = AppUrl in appsettings.json.
    [StringLength(300)] public string? LoginUrl { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

public class NewsPost
{
    public int Id { get; set; }
    [Required, StringLength(200)] public string TitleEn { get; set; } = "";
    [StringLength(200)] public string? TitleBn { get; set; }
    [Required] public string BodyEn { get; set; } = "";
    public string? BodyBn { get; set; }
    [StringLength(300)] public string? ImagePath { get; set; }
    public DateTime PublishedOn { get; set; } = DateTime.Today;
    public bool IsPublished { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public class Popup
{
    public int Id { get; set; }
    [Required, StringLength(200)] public string TitleEn { get; set; } = "";
    [StringLength(200)] public string? TitleBn { get; set; }
    [StringLength(1000)] public string? TextEn { get; set; }
    [StringLength(1000)] public string? TextBn { get; set; }
    [StringLength(300)] public string? ImagePath { get; set; }
    [StringLength(300)] public string? LinkUrl { get; set; }
    [StringLength(60)] public string? LinkTextEn { get; set; }
    [StringLength(60)] public string? LinkTextBn { get; set; }
    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime EndDate { get; set; } = DateTime.Today.AddDays(14);
    public bool IsActive { get; set; } = true;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

public class DemoRequest
{
    public int Id { get; set; }
    [Required, StringLength(120)] public string Name { get; set; } = "";
    [Required, StringLength(160)] public string Company { get; set; } = "";
    [Required, StringLength(40)] public string Phone { get; set; } = "";
    [StringLength(120), EmailAddress] public string? Email { get; set; }
    [StringLength(30)] public string? Employees { get; set; }
    [StringLength(200)] public string? Modules { get; set; }      // e.g. "HRM,Payroll,Inventory"
    [StringLength(1000)] public string? Message { get; set; }
    [StringLength(2), Required] public string Language { get; set; } = "en";
    public bool Contacted { get; set; }
    [StringLength(1000)] public string? AdminNote { get; set; }   // only seen in Admin
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
