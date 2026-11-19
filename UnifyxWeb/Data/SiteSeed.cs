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
}
