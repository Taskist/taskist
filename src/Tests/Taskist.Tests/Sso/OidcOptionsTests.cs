using Taskist.Web.Infrastructure;

namespace Taskist.Tests.Sso;

public class OidcOptionsTests
{
    #region Tests

    [Fact]
    public void Defaults_AreOff()
    {
        var options = new OidcOptions();

        Assert.False(options.IsActive);
        Assert.False(options.IsMisconfigured);
        Assert.False(options.LocalLoginRestricted);
        Assert.False(options.AutoProvision);
        Assert.True(options.RequireVerifiedEmail);
        Assert.True(options.RequireHttpsMetadata);
    }

    [Fact]
    public void Enabled_WithHttpsAuthorityAndClientId_IsActive()
    {
        var options = new OidcOptions { Enabled = true, Authority = " https://id.example.com/realms/acme ", ClientId = "taskist" };

        Assert.True(options.IsActive);
        Assert.False(options.IsMisconfigured);
    }

    [Theory]
    [InlineData(null, "taskist")]
    [InlineData("not a url", "taskist")]
    [InlineData("https://id.example.com", "")]
    [InlineData("https://id.example.com", "  ")]
    [InlineData("http://id.example.com", "taskist")]
    [InlineData("https://user:pass@id.example.com", "taskist")]
    [InlineData("https://id.example.com/?tenant=x", "taskist")]
    [InlineData("ftp://id.example.com", "taskist")]
    public void Enabled_WithInvalidSettings_IsMisconfigured(string? authority, string clientId)
    {
        var options = new OidcOptions { Enabled = true, Authority = authority, ClientId = clientId };

        Assert.False(options.IsActive);
        Assert.True(options.IsMisconfigured);
    }

    [Fact]
    public void HttpAuthority_IsAllowed_OnlyWhenHttpsMetadataIsNotRequired()
    {
        var options = new OidcOptions { Enabled = true, Authority = "http://localhost:8081/realms/dev", ClientId = "taskist", RequireHttpsMetadata = false };

        Assert.True(options.IsActive);
    }

    [Fact]
    public void LocalLogin_IsRestricted_OnlyWhenActiveAndDisabled()
    {
        var inactive = new OidcOptions { Enabled = false, DisableLocalLogin = true, Authority = "https://id.example.com", ClientId = "taskist" };
        var active = new OidcOptions { Enabled = true, DisableLocalLogin = true, Authority = "https://id.example.com", ClientId = "taskist" };

        Assert.False(inactive.LocalLoginRestricted);
        Assert.True(active.LocalLoginRestricted);
    }

    [Fact]
    public void Scopes_DefaultToOpenIdProfileEmail()
    {
        Assert.Equal(["openid", "profile", "email"], new OidcOptions().EffectiveScopes);
    }

    [Fact]
    public void Scopes_AlwaysIncludeOpenId_AndRemoveDuplicates()
    {
        var options = new OidcOptions { Scopes = "profile, email email groups" };

        Assert.Equal(["openid", "profile", "email", "groups"], options.EffectiveScopes);
    }

    [Fact]
    public void DisplayName_FallsBack_WhenEmpty()
    {
        Assert.Equal("Single sign-on", new OidcOptions { DisplayName = " " }.EffectiveDisplayName);
        Assert.Equal("Keycloak", new OidcOptions { DisplayName = " Keycloak " }.EffectiveDisplayName);
    }

    [Fact]
    public void RoleClaim_IsNull_WhenBlank()
    {
        Assert.Null(new OidcOptions { RoleClaim = "  " }.EffectiveRoleClaim);
        Assert.Equal("groups", new OidcOptions { RoleClaim = " groups " }.EffectiveRoleClaim);
    }

    #endregion
}
