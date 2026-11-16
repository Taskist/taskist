namespace Taskist.Web.Infrastructure;

public class OidcOptions
{
    #region Fields

    public const string SectionName = "Authentication:Oidc";

    public const string SchemeName = "oidc";

    public const string ExternalCookieScheme = "Taskist.External";

    public const string CallbackPath = "/signin-oidc";

    private const string DefaultScopes = "openid profile email";

    #endregion

    #region Properties

    public bool Enabled { get; set; }

    public string? DisplayName { get; set; }

    public string? Authority { get; set; }

    public string? ClientId { get; set; }

    public string? ClientSecret { get; set; }

    public string? Scopes { get; set; }

    public bool RequireHttpsMetadata { get; set; } = true;

    public bool RequireVerifiedEmail { get; set; } = true;

    public bool AutoProvision { get; set; }

    public string? RoleClaim { get; set; }

    public Dictionary<string, string> RoleMappings { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public bool DisableLocalLogin { get; set; }

    public bool HasValidAuthority =>
        Uri.TryCreate(Authority?.Trim(), UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttps || (uri.Scheme == Uri.UriSchemeHttp && !RequireHttpsMetadata))
        && string.IsNullOrEmpty(uri.UserInfo)
        && string.IsNullOrEmpty(uri.Query)
        && string.IsNullOrEmpty(uri.Fragment);

    public bool HasClientId => !string.IsNullOrWhiteSpace(ClientId);

    public bool IsActive => Enabled && HasValidAuthority && HasClientId;

    public bool IsMisconfigured => Enabled && !IsActive;

    public bool LocalLoginRestricted => IsActive && DisableLocalLogin;

    public string EffectiveDisplayName => string.IsNullOrWhiteSpace(DisplayName) ? "Single sign-on" : DisplayName.Trim();

    public string? EffectiveRoleClaim => string.IsNullOrWhiteSpace(RoleClaim) ? null : RoleClaim.Trim();

    public IReadOnlyList<string> EffectiveScopes
    {
        get
        {
            var scopes = (string.IsNullOrWhiteSpace(Scopes) ? DefaultScopes : Scopes)
                .Split([' ', ',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (!scopes.Contains("openid", StringComparer.Ordinal))
                scopes.Insert(0, "openid");

            return scopes;
        }
    }

    #endregion
}
