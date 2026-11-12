namespace Taskist.Web.Helpers.TwoFactor;

public static class TwoFactorSetupPolicy
{
    #region Fields

    private static readonly HashSet<string> AllowedControllers = new(StringComparer.OrdinalIgnoreCase)
    {
        "Profile", "Account", "Notification", "Common"
    };

    #endregion

    #region Methods

    public static bool IsSetupRequired(bool requireForAdministrators, bool isAdministrator, string? authMethod) =>
        requireForAdministrators && isAdministrator && string.Equals(authMethod, AuthMethod.Local, StringComparison.Ordinal);

    public static bool IsAllowedDuringSetup(string? controller) =>
        !string.IsNullOrEmpty(controller) && AllowedControllers.Contains(controller);

    #endregion
}
