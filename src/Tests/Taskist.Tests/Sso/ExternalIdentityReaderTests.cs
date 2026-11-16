using System.Security.Claims;
using Taskist.Web.Helpers.Sso;

namespace Taskist.Tests.Sso;

public class ExternalIdentityReaderTests
{
    #region Fields

    private const string Issuer = "https://id.example.com/realms/acme";

    #endregion

    #region Tests

    [Fact]
    public void Read_MapsStandardClaims()
    {
        var identity = ExternalIdentityReader.Read(
        [
            new Claim("iss", Issuer + "/"),
            new Claim("sub", " 1234 "),
            new Claim("email", "ana@example.com"),
            new Claim("email_verified", "true"),
            new Claim("given_name", "Ana"),
            new Claim("family_name", "Silva"),
            new Claim("name", "Ana Silva")
        ], roleClaim: null);

        Assert.Equal(Issuer, identity.Issuer);
        Assert.Equal("1234", identity.Subject);
        Assert.Equal("ana@example.com", identity.Email);
        Assert.True(identity.EmailVerified);
        Assert.Equal("Ana", identity.GivenName);
        Assert.Equal("Silva", identity.FamilyName);
        Assert.Equal("Ana Silva", identity.Name);
        Assert.Empty(identity.Groups);
    }

    [Fact]
    public void Read_UsesClaimIssuer_WhenIssClaimIsMissing()
    {
        var identity = ExternalIdentityReader.Read([new Claim("sub", "1", ClaimValueTypes.String, Issuer)], null);

        Assert.Equal(Issuer, identity.Issuer);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("True", true)]
    [InlineData("1", true)]
    [InlineData("false", false)]
    [InlineData("0", false)]
    [InlineData("yes", false)]
    [InlineData("", false)]
    public void Read_ParsesEmailVerified(string value, bool expected)
    {
        var identity = ExternalIdentityReader.Read([new Claim("sub", "1"), new Claim("email_verified", value)], null);

        Assert.Equal(expected, identity.EmailVerified);
    }

    [Fact]
    public void Read_MissingEmailVerified_IsNotVerified()
    {
        var identity = ExternalIdentityReader.Read([new Claim("sub", "1"), new Claim("email", "ana@example.com")], null);

        Assert.False(identity.EmailVerified);
    }

    [Fact]
    public void Read_CollectsGroups_FromConfiguredClaim_Distinct()
    {
        var identity = ExternalIdentityReader.Read(
        [
            new Claim("sub", "1"),
            new Claim("groups", "taskist-leads"),
            new Claim("groups", "TASKIST-LEADS"),
            new Claim("groups", " devs "),
            new Claim("groups", ""),
            new Claim("roles", "ignored")
        ], "groups");

        Assert.Equal(["taskist-leads", "devs"], identity.Groups);
    }

    [Fact]
    public void Read_NoGroups_WhenRoleClaimIsNotConfigured()
    {
        var identity = ExternalIdentityReader.Read([new Claim("sub", "1"), new Claim("groups", "devs")], null);

        Assert.Empty(identity.Groups);
    }

    [Fact]
    public void Read_EmptyClaims_GivesEmptyIdentity()
    {
        var identity = ExternalIdentityReader.Read([], null);

        Assert.Equal(string.Empty, identity.Subject);
        Assert.Equal(string.Empty, identity.Issuer);
        Assert.Null(identity.Email);
    }

    #endregion
}
