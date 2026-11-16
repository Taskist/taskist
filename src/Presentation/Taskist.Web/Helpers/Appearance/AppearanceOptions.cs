namespace Taskist.Web.Helpers.Appearance;

public static class AppearanceOptions
{
    #region Constants

    public const string CookieName = "taskist.appearance";
    public const string ModeAttribute = "Appearance.Mode";
    public const string AccentAttribute = "Appearance.Accent";

    public const string Light = "light";
    public const string Dark = "dark";
    public const string Auto = "auto";

    public const string DefaultMode = Light;
    public const string DefaultAccent = "indigo";

    #endregion

    #region Lists

    public static readonly IReadOnlyList<string> Modes = [Light, Dark, Auto];

    public static readonly IReadOnlyList<string> Accents = ["indigo", "teal", "blue", "purple", "green", "orange", "rose"];

    #endregion

    #region Methods

    public static string NormalizeMode(string? mode) =>
        Modes.FirstOrDefault(m => string.Equals(m, mode?.Trim(), StringComparison.OrdinalIgnoreCase)) ?? DefaultMode;

    public static string NormalizeAccent(string? accent) =>
        Accents.FirstOrDefault(a => string.Equals(a, accent?.Trim(), StringComparison.OrdinalIgnoreCase)) ?? DefaultAccent;

    #endregion
}
