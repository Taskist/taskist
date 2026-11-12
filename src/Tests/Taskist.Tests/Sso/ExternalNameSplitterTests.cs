using Taskist.Web.Helpers.Sso;
using Taskist.Web.Models.Users;

namespace Taskist.Tests.Sso;

public class ExternalNameSplitterTests
{
    #region Tests

    [Fact]
    public void UsesGivenAndFamilyName()
    {
        var (first, last) = ExternalNameSplitter.Split(new ExternalIdentity { GivenName = " Ana ", FamilyName = "Silva", Name = "Someone Else" });

        Assert.Equal("Ana", first);
        Assert.Equal("Silva", last);
    }

    [Fact]
    public void SplitsFullName_WhenGivenNameIsMissing()
    {
        var (first, last) = ExternalNameSplitter.Split(new ExternalIdentity { Name = "Ana Maria Silva" });

        Assert.Equal("Ana", first);
        Assert.Equal("Maria Silva", last);
    }

    [Fact]
    public void FallsBackToEmailLocalPart()
    {
        var (first, last) = ExternalNameSplitter.Split(new ExternalIdentity { Email = "ana.silva@example.com" });

        Assert.Equal("ana.silva", first);
        Assert.Equal(string.Empty, last);
    }

    [Fact]
    public void FallsBackToUser_WhenNothingIsKnown()
    {
        var (first, _) = ExternalNameSplitter.Split(new ExternalIdentity());

        Assert.Equal("User", first);
    }

    [Fact]
    public void TruncatesToColumnLength()
    {
        var (first, last) = ExternalNameSplitter.Split(new ExternalIdentity { GivenName = new string('a', 80), FamilyName = new string('b', 80) });

        Assert.Equal(ExternalNameSplitter.MaxLength, first.Length);
        Assert.Equal(ExternalNameSplitter.MaxLength, last.Length);
    }

    [Theory]
    [InlineData("https://id.example.com/realms/acme", "id.example.com")]
    [InlineData("not-a-url", "not-a-url")]
    [InlineData("", "")]
    public void ProviderName_IsTheIssuerHost(string issuer, string expected)
    {
        Assert.Equal(expected, LinkedSignInModel.ProviderNameFrom(issuer));
    }

    [Theory]
    [InlineData("NotProvisioned", "NotProvisioned")]
    [InlineData(" Failed ", "Failed")]
    [InlineData("<script>", null)]
    [InlineData("notprovisioned", null)]
    [InlineData(null, null)]
    public void ErrorCode_AcceptsOnlyKnownCodes(string? code, string? expected)
    {
        Assert.Equal(expected, SsoErrorCode.Normalize(code));
    }

    #endregion
}
