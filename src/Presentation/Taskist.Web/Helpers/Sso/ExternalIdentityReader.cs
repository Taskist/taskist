using System.Security.Claims;

namespace Taskist.Web.Helpers.Sso;

public static class ExternalIdentityReader
{
    #region Methods

    public static ExternalIdentity Read(IEnumerable<Claim> claims, string? roleClaim)
    {
        var list = claims?.ToList() ?? [];

        var subjectClaim = list.FirstOrDefault(c => c.Type == "sub");
        var issuer = Value(list, "iss") ?? subjectClaim?.Issuer ?? string.Empty;

        var groups = string.IsNullOrWhiteSpace(roleClaim)
            ? []
            : list.Where(c => c.Type == roleClaim)
                .Select(c => c.Value?.Trim())
                .Where(v => !string.IsNullOrEmpty(v))
                .Select(v => v!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

        return new ExternalIdentity
        {
            Issuer = NormalizeIssuer(issuer),
            Subject = subjectClaim?.Value?.Trim() ?? string.Empty,
            Email = Value(list, "email"),
            EmailVerified = IsTrue(Value(list, "email_verified")),
            GivenName = Value(list, "given_name"),
            FamilyName = Value(list, "family_name"),
            Name = Value(list, "name"),
            Groups = groups
        };
    }

    public static string NormalizeIssuer(string? issuer)
    {
        if (string.IsNullOrWhiteSpace(issuer))
            return string.Empty;

        return issuer.Trim().TrimEnd('/');
    }

    public static bool IsTrue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var trimmed = value.Trim();
        return trimmed.Equals("true", StringComparison.OrdinalIgnoreCase) || trimmed == "1";
    }

    private static string? Value(List<Claim> claims, string type)
    {
        var value = claims.FirstOrDefault(c => c.Type == type)?.Value?.Trim();
        return string.IsNullOrEmpty(value) ? null : value;
    }

    #endregion
}
