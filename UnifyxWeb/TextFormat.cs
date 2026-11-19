using System.Net;
using Microsoft.AspNetCore.Html;

namespace UnifyxWeb;

// Plain text typed in Admin → safe paragraphs (no HTML from the text is kept).
public static class TextFormat
{
    public static IHtmlContent Paragraphs(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return HtmlString.Empty;
        var parts = text.Replace("\r\n", "\n").Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(p => "<p>" + WebUtility.HtmlEncode(p).Replace("\n", "<br/>") + "</p>");
        return new HtmlString(string.Concat(parts));
    }
}
