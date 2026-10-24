using System.Net;
using System.Text.RegularExpressions;

namespace Taskist.Core.Common;

public static partial class HtmlText
{
    #region Methods

    public static bool HasContent(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return false;

        if (MediaTagRegex().IsMatch(html))
            return true;

        var text = WebUtility.HtmlDecode(TagRegex().Replace(html, string.Empty));
        return text.Replace('\u00A0', ' ').Trim().Length > 0;
    }

    public static string ToPlainText(string? html, int maxLength = 0)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        var withBreaks = Regex.Replace(html, @"<\s*(br|/p|/div|/li|/h\d)\s*/?\s*>", "\n", RegexOptions.IgnoreCase);
        withBreaks = Regex.Replace(withBreaks, @"<\s*li[^>]*>", "- ", RegexOptions.IgnoreCase);
        var text = WebUtility.HtmlDecode(TagRegex().Replace(withBreaks, string.Empty)).Replace(' ', ' ');
        text = Regex.Replace(text, @"[ \t]+\n", "\n");
        text = Regex.Replace(text, @"\n{3,}", "\n\n").Trim();

        return maxLength > 0 && text.Length > maxLength ? text[..maxLength].TrimEnd() + "…" : text;
    }

    public static string FromPlainText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        if (TagRegex().IsMatch(text) && text.TrimStart().StartsWith('<'))
            return text;

        var lines = text.Replace("\r\n", "\n").Split('\n');
        return string.Join("<br>", lines.Select(WebUtility.HtmlEncode));
    }

    #endregion

    #region Utilities

    [GeneratedRegex("<[^>]+>")]
    private static partial Regex TagRegex();

    [GeneratedRegex(@"<(img|video|table)\b", RegexOptions.IgnoreCase)]
    private static partial Regex MediaTagRegex();

    #endregion
}
