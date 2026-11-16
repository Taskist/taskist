namespace Taskist.Web.Helpers.Sso;

public static class ExternalNameSplitter
{
    #region Fields

    public const int MaxLength = 50;

    #endregion

    #region Methods

    public static (string FirstName, string LastName) Split(ExternalIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);

        var first = identity.GivenName?.Trim();
        var last = identity.FamilyName?.Trim();

        if (string.IsNullOrEmpty(first) && !string.IsNullOrWhiteSpace(identity.Name))
        {
            var parts = identity.Name.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            first = parts[0];
            if (string.IsNullOrEmpty(last) && parts.Length > 1)
                last = parts[1];
        }

        if (string.IsNullOrEmpty(first) && !string.IsNullOrWhiteSpace(identity.Email))
            first = identity.Email.Split('@')[0].Trim();

        if (string.IsNullOrEmpty(first))
            first = "User";

        return (Truncate(first), Truncate(last ?? string.Empty));
    }

    private static string Truncate(string value) => value.Length > MaxLength ? value[..MaxLength] : value;

    #endregion
}
