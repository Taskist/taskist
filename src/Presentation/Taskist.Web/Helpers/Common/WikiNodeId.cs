using System.Globalization;

namespace Taskist.Web.Helpers.Common;

public static class WikiNodeId
{
    #region Fields

    public const string PagePrefix = "page_";

    public const string CategoryPrefix = "cat_";

    #endregion

    #region Methods

    public static bool TryParse(string? value, string prefix, out int id)
    {
        id = 0;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var number = value.Trim();
        if (number.StartsWith(prefix, StringComparison.Ordinal))
            number = number[prefix.Length..];

        return int.TryParse(number, NumberStyles.None, CultureInfo.InvariantCulture, out id) && id > 0;
    }

    public static bool TryParsePage(string? value, out int id) => TryParse(value, PagePrefix, out id);

    public static bool TryParseCategory(string? value, out int id) => TryParse(value, CategoryPrefix, out id);

    #endregion
}
