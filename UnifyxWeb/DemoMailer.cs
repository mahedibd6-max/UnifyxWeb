using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Options;
using UnifyxWeb.Data;

namespace UnifyxWeb;

// Mail server settings. The password is never in appsettings.json:
// dotnet user-secrets set "Smtp:Password" "<password>" --project UnifyxWeb
public class SmtpOptions
{
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string User { get; set; } = "";
    public string Password { get; set; } = "";
    public string From { get; set; } = "";
    public string NotifyTo { get; set; } = "";   // empty: the email in Admin → Contact & links
}

// Emails the team when a demo request arrives. If mail is not set up, or sending fails,
// the request is still saved and shows in Admin — the visitor never sees an error.
public class DemoMailer
{
    private readonly SmtpOptions _o;
    private readonly SiteInfo _site;
    private readonly ILogger<DemoMailer> _log;

    public DemoMailer(IOptions<SmtpOptions> o, SiteInfo site, ILogger<DemoMailer> log)
    {
        _o = o.Value; _site = site; _log = log;
    }

    public bool IsSetUp => !string.IsNullOrWhiteSpace(_o.Host) && !string.IsNullOrWhiteSpace(_o.From);

    public async Task NotifyAsync(DemoRequest r, string adminLink)
    {
        if (!IsSetUp) { _log.LogInformation("Demo request {Id} saved; email not set up.", r.Id); return; }
        string to = string.IsNullOrWhiteSpace(_o.NotifyTo) ? _site.Value.Email : _o.NotifyTo;
        try
        {
            var body = new StringBuilder()
                .AppendLine("A new demo request came from the website.").AppendLine()
                .AppendLine($"Name:      {r.Name}")
                .AppendLine($"Company:   {r.Company}")
                .AppendLine($"Phone:     {r.Phone}")
                .AppendLine($"Email:     {r.Email}")
                .AppendLine($"Employees: {r.Employees}")
                .AppendLine($"Modules:   {r.Modules}")
                .AppendLine($"Language:  {(r.Language == "bn" ? "Bangla" : "English")}")
                .AppendLine($"Time:      {r.CreatedAt:dd MMM yyyy, h:mm tt}").AppendLine()
                .AppendLine("Message:").AppendLine(r.Message).AppendLine()
                .AppendLine($"Open in Admin: {adminLink}");

            using var msg = new MailMessage(_o.From, to)
            {
                Subject = OneLine($"New demo request: {r.Company}"),
                Body = body.ToString(),
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };
            if (!string.IsNullOrWhiteSpace(r.Email))
                msg.ReplyToList.Add(new MailAddress(r.Email, OneLine(r.Name)));

            using var smtp = new SmtpClient(_o.Host, _o.Port) { EnableSsl = _o.EnableSsl, Timeout = 15000 };
            if (!string.IsNullOrWhiteSpace(_o.User))
                smtp.Credentials = new NetworkCredential(_o.User, _o.Password);
            await smtp.SendMailAsync(msg);
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Demo request {Id} saved, but the email could not be sent.", r.Id);
        }
    }

    private static string OneLine(string s) => s.Replace('\r', ' ').Replace('\n', ' ').Trim();
}
