using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace UnifyxWeb.Data;

public static class SiteSeed
{
    // Applies migrations and creates the first admin from configuration (user-secrets),
    // never from code. Development: right-click UnifyxWeb → Manage User Secrets:
    //   { "AdminSeed": { "Email": "you@example.com", "Password": "..." } }
    public static async Task RunAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("SiteSeed");
        try
        {
            var db = sp.GetRequiredService<SiteDbContext>();
            await db.Database.MigrateAsync();

            if (!await db.SiteSettings.AnyAsync())
            {
                db.SiteSettings.Add(new SiteSetting());
                await db.SaveChangesAsync();
            }

            await AddShopProductAsync(db);

            var cfg = sp.GetRequiredService<IConfiguration>();
            string? email = cfg["AdminSeed:Email"], password = cfg["AdminSeed:Password"];
            var users = sp.GetRequiredService<UserManager<IdentityUser>>();
            if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password)
                && await users.FindByEmailAsync(email) == null)
            {
                var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                var result = await users.CreateAsync(user, password);
                if (!result.Succeeded)
                    logger.LogWarning("Admin not created: {Errors}", string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }
        catch (Exception ex)
        {
            // The public pages still work without the database.
            logger.LogError(ex, "Website database is not ready.");
        }
    }

    // Adds "Unifyx Shop" to the product list once. After that it is edited (or deleted) in Admin → Products
    // like any other product; the marker row stops it from coming back after a delete.
    private static async Task AddShopProductAsync(SiteDbContext db)
    {
        const string marker = "seed.product.unifyx-shop";
        if (await db.SiteTexts.AnyAsync(x => x.Key == marker)) return;
        if (!await db.Products.AnyAsync(x => x.Slug == "unifyx-shop"))
        {
            int order = (await db.Products.MaxAsync(x => (int?)x.SortOrder) ?? 0) + 1;
            db.Products.Add(new Product
            {
                Slug = "unifyx-shop",
                NameEn = "Unifyx Shop",
                NameBn = "Unifyx Shop — দোকানের সফটওয়্যার",
                Icon = "🛍️",
                TaglineEn = "The complete account of your shop — in one place. 14 days free.",
                TaglineBn = "আপনার দোকানের পুরো হিসাব — এক জায়গায়। ১৪ দিন ফ্রি।",
                DescriptionEn = "Unifyx Shop is made for mobile phone, electronics and accessories shops. Sell with a barcode or IMEI scan, " +
                    "keep every customer's due, check warranty in seconds, buy and exchange used phones, run repair jobs, " +
                    "and see today's sales, profit and cash on one screen. It runs in the browser on any computer — no installation, " +
                    "and selling keeps going even when the internet drops. Start with a free 14-day trial, no card needed. " +
                    "Plans from Tk 500 a month; pay 10 months and get 12.",
                DescriptionBn = "Unifyx Shop বানানো হয়েছে মোবাইল, ইলেকট্রনিক্স আর এক্সেসরিজের দোকানের জন্য। বারকোড বা IMEI স্ক্যান করে বিক্রি করুন, " +
                    "প্রতিটা Customer-এর বাকির হিসাব রাখুন, কয়েক সেকেন্ডে ওয়ারেন্টি দেখুন, পুরনো ফোন কিনুন ও বদল করুন, মেরামতের কাজ চালান, " +
                    "আর আজকের বিক্রি, লাভ ও ক্যাশ এক পাতায় দেখুন। যেকোনো কম্পিউটারের ব্রাউজারে চলে — কিছু ইনস্টল করতে হয় না, " +
                    "ইন্টারনেট চলে গেলেও বিক্রি থামে না। ১৪ দিন ফ্রি ব্যবহার করুন, কার্ড লাগবে না। " +
                    "মাসে ৫০০ টাকা থেকে শুরু; ১০ মাসের টাকায় ১২ মাস।",
                FeaturesEn = string.Join("\n", new[]
                {
                    "🧾 | Fast selling | Barcode or IMEI scan, receipt in seconds | Shift open and close; X / Z report; Works offline",
                    "📱 | IMEI tracking | Every phone kept by its IMEI | Bought from whom; In stock or sold; Search by IMEI-2 too",
                    "📒 | Customer due | Sell on credit and collect later | Credit limit; Due receipt; WhatsApp reminder",
                    "🛡️ | Warranty | Check warranty in seconds | By IMEI or receipt; Claim job card; Status and slip",
                    "🔄 | Used phones | Buy used phones and exchange | Cash buy; Exchange against new; Own price and warranty",
                    "🛠️ | Repairs | Repair job cards from receive to deliver | Parts from stock; Labour charge; Paid into the shift",
                    "🏷️ | Offers and discounts | Discounts with owner approval | Buy X get Y; Coupons; Minimum price check",
                    "📊 | My Shop | The owner's dashboard | Today's sales and profit; Cash and dues; Low stock and best sellers"
                }),
                FeaturesBn = string.Join("\n", new[]
                {
                    "🧾 | দ্রুত বিক্রি | বারকোড বা IMEI স্ক্যান, কয়েক সেকেন্ডে রসিদ | শিফট খোলা-বন্ধ; X / Z রিপোর্ট; ইন্টারনেট ছাড়াও চলে",
                    "📱 | IMEI হিসাব | প্রতিটা ফোন তার IMEI ধরে | কার কাছ থেকে কেনা; স্টকে আছে না বিক্রি; IMEI-2 দিয়েও খোঁজা",
                    "📒 | বাকির খাতা | বাকিতে বিক্রি, পরে আদায় | বাকির সীমা; আদায়ের রসিদ; WhatsApp-এ মনে করানো",
                    "🛡️ | ওয়ারেন্টি | কয়েক সেকেন্ডে ওয়ারেন্টি দেখা | IMEI বা রসিদ দিয়ে; ক্লেইমের জব কার্ড; স্ট্যাটাস ও স্লিপ",
                    "🔄 | পুরনো ফোন | পুরনো ফোন কেনা ও বদল | নগদে কেনা; নতুনের সাথে Exchange; নিজস্ব দাম ও ওয়ারেন্টি",
                    "🛠️ | মেরামত | জমা থেকে ডেলিভারি পর্যন্ত জব কার্ড | স্টক থেকে পার্টস; মজুরি; টাকা শিফটে যোগ",
                    "🏷️ | অফার ও ছাড় | মালিকের অনুমতিসহ ছাড় | ১টা কিনলে ১টা ফ্রি; কুপন; সর্বনিম্ন দামের নিয়ন্ত্রণ",
                    "📊 | My Shop | মালিকের ড্যাশবোর্ড | আজকের বিক্রি ও লাভ; ক্যাশ ও বাকি; কম স্টক ও বেশি বিক্রি"
                }),
                AppUrl = "https://app.unifyxerp.com/Signup",
                SortOrder = order,
                IsActive = true
            });
        }
        db.SiteTexts.Add(new SiteText { Key = marker, TextEn = "added", TextBn = "added" });
        await db.SaveChangesAsync();
    }
}
