using System.Text.RegularExpressions;

namespace Taskist.Service.Wiki;

public static partial class WikiImageReference
{
    #region Methods

    public static int? ExtractId(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return null;

        var match = ImageUrlPattern().Match(url);
        return match.Success && int.TryParse(match.Groups[1].Value, out var id) && id > 0 ? id : null;
    }

    public static bool References(string? html, int documentId)
    {
        if (string.IsNullOrEmpty(html) || documentId <= 0)
            return false;

        foreach (Match match in ImageUrlPattern().Matches(html))
        {
            if (int.TryParse(match.Groups[1].Value, out var id) && id == documentId)
                return true;
        }

        return false;
    }

    public static string[] SearchFragments(int documentId) =>
        [$"WikiDocument/Image/{documentId}", $"WikiDocument/Image?id={documentId}"];

    #endregion

    #region Utilities

    [GeneratedRegex(@"WikiDocument/Image(?:/|\?id=)(\d+)(?!\d)", RegexOptions.IgnoreCase)]
    private static partial Regex ImageUrlPattern();

    #endregion
}
