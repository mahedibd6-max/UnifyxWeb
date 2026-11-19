using System.Text;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using UnifyxWeb.Data;

namespace UnifyxWeb;

// Search engines: /robots.txt and /sitemap.xml, built from the live site (both languages, products and news).
public static class Seo
{
    // The site's own address as the visitor reached it (localhost, a tunnel or unifyxerp.com).
    public static string Origin(HttpRequest r) => $"{r.Scheme}://{r.Host}";

    public static void MapSeo(this WebApplication app)
    {
        app.MapGet("/robots.txt", (HttpContext ctx) =>
        {
            var o = Origin(ctx.Request);
            string text = "User-agent: *\nAllow: /\nDisallow: /admin\nDisallow: /Admin\n\nSitemap: " + o + "/sitemap.xml\n";
            return Results.Text(text, "text/plain; charset=utf-8");
        });

        app.MapGet("/sitemap.xml", async (HttpContext ctx, SiteDbContext db) =>
        {
            var o = Origin(ctx.Request);
            var pages = new List<(string Path, DateTime? Changed, string Freq, string Priority)>
            {
                ("", null, "weekly", "1.0"),
                ("Pricing", null, "monthly", "0.9"),
                ("Products", null, "monthly", "0.8"),
                ("Contact", null, "yearly", "0.8"),
                ("News", null, "weekly", "0.6"),
                ("Privacy", null, "yearly", "0.2"),
                ("Terms", null, "yearly", "0.2"),
            };
            try
            {
                foreach (var p in await db.Products.AsNoTracking().Where(x => x.IsActive)
                             .Select(x => new { x.Slug, x.UpdatedAt }).ToListAsync())
                    pages.Add(("Products/" + p.Slug, p.UpdatedAt, "monthly", "0.7"));
                foreach (var n in await db.NewsPosts.AsNoTracking().Where(x => x.IsPublished && x.PublishedOn <= DateTime.Today)
                             .Select(x => new { x.Id, x.PublishedOn }).ToListAsync())
                    pages.Add(("News/" + n.Id, n.PublishedOn, "yearly", "0.5"));
            }
            catch (Exception) { /* the fixed pages are still listed */ }

            var sb = new StringBuilder();
            using (var w = XmlWriter.Create(sb, new XmlWriterSettings { Indent = true, Encoding = Encoding.UTF8 }))
            {
                w.WriteStartDocument();
                w.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");
                w.WriteAttributeString("xmlns", "xhtml", null, "http://www.w3.org/1999/xhtml");
                foreach (var (path, changed, freq, priority) in pages)
                    foreach (var lang in new[] { "en", "bn" })
                    {
                        w.WriteStartElement("url");
                        w.WriteElementString("loc", Url(o, lang, path));
                        if (changed.HasValue) w.WriteElementString("lastmod", changed.Value.ToString("yyyy-MM-dd"));
                        w.WriteElementString("changefreq", freq);
                        w.WriteElementString("priority", priority);
                        foreach (var alt in new[] { "en", "bn" })
                        {
                            w.WriteStartElement("xhtml", "link", "http://www.w3.org/1999/xhtml");
                            w.WriteAttributeString("rel", "alternate");
                            w.WriteAttributeString("hreflang", alt);
                            w.WriteAttributeString("href", Url(o, alt, path));
                            w.WriteEndElement();
                        }
                        w.WriteEndElement();
                    }
                w.WriteEndElement();
                w.WriteEndDocument();
            }
            return Results.Text(sb.ToString().Replace("utf-16", "utf-8"), "application/xml; charset=utf-8");
        });
    }

    public static string Url(string origin, string lang, string path) =>
        origin + "/" + lang + (string.IsNullOrEmpty(path) ? "" : "/" + path);
}
