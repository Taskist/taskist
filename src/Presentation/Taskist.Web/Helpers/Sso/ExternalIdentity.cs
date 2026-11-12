namespace Taskist.Web.Helpers.Sso;

public class ExternalIdentity
{
    #region Properties

    public string Issuer { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string? Email { get; set; }

    public bool EmailVerified { get; set; }

    public string? GivenName { get; set; }

    public string? FamilyName { get; set; }

    public string? Name { get; set; }

    public IReadOnlyList<string> Groups { get; set; } = [];

    #endregion
}
