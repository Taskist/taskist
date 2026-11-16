namespace Taskist.Web.Models.Users;

public class LinkedSignInModel
{
    #region Properties

    public int Id { get; set; }

    public string ProviderName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? LastUsedOn { get; set; }

    #endregion

    #region Methods

    public static string ProviderNameFrom(string? issuer)
    {
        if (string.IsNullOrWhiteSpace(issuer))
            return string.Empty;

        return Uri.TryCreate(issuer, UriKind.Absolute, out var uri) ? uri.Host : issuer;
    }

    #endregion
}
