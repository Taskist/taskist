namespace Taskist.Web.Helpers.Sso;

public static class LocalLoginSwitch
{
    #region Methods

    public static bool IsOn(string? value)
    {
        var text = value?.Trim();

        return text == "1" || string.Equals(text, bool.TrueString, StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}
