namespace Taskist.Web.Helpers.Sso;

public static class SsoErrorCode
{
    #region Fields

    public const string Failed = "Failed";

    public const string Denied = "Denied";

    public const string Unavailable = "Unavailable";

    public const string MissingSubject = "MissingSubject";

    public const string MissingEmail = "MissingEmail";

    public const string UnverifiedEmail = "UnverifiedEmail";

    public const string LinkConflict = "LinkConflict";

    public const string NotProvisioned = "NotProvisioned";

    private static readonly HashSet<string> Known = new(StringComparer.Ordinal)
    {
        Failed, Denied, Unavailable, MissingSubject, MissingEmail, UnverifiedEmail, LinkConflict, NotProvisioned
    };

    #endregion

    #region Methods

    public static string? FromDecision(SsoSignInDecision decision) => decision switch
    {
        SsoSignInDecision.RejectMissingSubject => MissingSubject,
        SsoSignInDecision.RejectMissingEmail => MissingEmail,
        SsoSignInDecision.RejectUnverifiedEmail => UnverifiedEmail,
        SsoSignInDecision.RejectLinkConflict => LinkConflict,
        SsoSignInDecision.RejectNotProvisioned => NotProvisioned,
        _ => null
    };

    public static string? Normalize(string? code) =>
        !string.IsNullOrWhiteSpace(code) && Known.Contains(code.Trim()) ? code.Trim() : null;

    public static string ResourceKey(string code) => $"Login.Sso.Error.{code}";

    #endregion
}
