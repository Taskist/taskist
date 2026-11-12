using Taskist.Web.Helpers.Sso;

namespace Taskist.Tests.Sso;

public class SsoSignInPlannerTests
{
    #region Utilities

    private static ExternalIdentity Identity(string? email = "ana@example.com", bool verified = true, string subject = "1234") => new()
    {
        Issuer = "https://id.example.com",
        Subject = subject,
        Email = email,
        EmailVerified = verified
    };

    private static SsoSignInDecision Decide(ExternalIdentity identity, bool linked = false, bool emailUser = false,
        bool emailUserLinked = false, bool requireVerified = true, bool autoProvision = false) =>
        SsoSignInPlanner.Decide(identity, linked, emailUser, emailUserLinked, requireVerified, autoProvision);

    #endregion

    #region Tests

    [Fact]
    public void LinkedUser_IsUsed_EvenWithoutVerifiedEmail()
    {
        Assert.Equal(SsoSignInDecision.UseLinkedUser, Decide(Identity(email: null, verified: false), linked: true));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void MissingSubject_IsRejected(string subject)
    {
        Assert.Equal(SsoSignInDecision.RejectMissingSubject, Decide(Identity(subject: subject), linked: true));
    }

    [Fact]
    public void MissingIssuer_IsRejected()
    {
        var identity = Identity();
        identity.Issuer = "";

        Assert.Equal(SsoSignInDecision.RejectMissingSubject, Decide(identity, linked: true));
    }

    [Fact]
    public void NotLinked_WithoutEmail_IsRejected()
    {
        Assert.Equal(SsoSignInDecision.RejectMissingEmail, Decide(Identity(email: null), emailUser: true, autoProvision: true));
    }

    [Fact]
    public void ExistingUser_WithVerifiedEmail_IsLinked()
    {
        Assert.Equal(SsoSignInDecision.LinkExistingUser, Decide(Identity(), emailUser: true));
    }

    [Fact]
    public void ExistingUser_WithUnverifiedEmail_IsNeverLinked()
    {
        Assert.Equal(SsoSignInDecision.RejectUnverifiedEmail, Decide(Identity(verified: false), emailUser: true, autoProvision: true));
    }

    [Fact]
    public void ExistingUser_WithUnverifiedEmail_IsLinked_WhenVerificationIsNotRequired()
    {
        Assert.Equal(SsoSignInDecision.LinkExistingUser, Decide(Identity(verified: false), emailUser: true, requireVerified: false));
    }

    [Fact]
    public void ExistingUser_AlreadyLinkedToAnotherAccountAtTheSameProvider_IsRejected()
    {
        Assert.Equal(SsoSignInDecision.RejectLinkConflict, Decide(Identity(), emailUser: true, emailUserLinked: true));
    }

    [Fact]
    public void UnknownUser_IsRejected_WhenAutoProvisioningIsOff()
    {
        Assert.Equal(SsoSignInDecision.RejectNotProvisioned, Decide(Identity()));
    }

    [Fact]
    public void UnknownUser_IsProvisioned_WhenAutoProvisioningIsOn()
    {
        Assert.Equal(SsoSignInDecision.ProvisionUser, Decide(Identity(), autoProvision: true));
    }

    [Fact]
    public void UnknownUser_WithUnverifiedEmail_IsNotProvisioned()
    {
        Assert.Equal(SsoSignInDecision.RejectUnverifiedEmail, Decide(Identity(verified: false), autoProvision: true));
    }

    [Theory]
    [InlineData(SsoSignInDecision.UseLinkedUser, false)]
    [InlineData(SsoSignInDecision.LinkExistingUser, false)]
    [InlineData(SsoSignInDecision.ProvisionUser, false)]
    [InlineData(SsoSignInDecision.RejectMissingEmail, true)]
    [InlineData(SsoSignInDecision.RejectNotProvisioned, true)]
    public void IsRejection_MatchesDecision(SsoSignInDecision decision, bool expected)
    {
        Assert.Equal(expected, SsoSignInPlanner.IsRejection(decision));
    }

    [Fact]
    public void EveryRejection_HasAnErrorCode()
    {
        foreach (var decision in Enum.GetValues<SsoSignInDecision>().Where(SsoSignInPlanner.IsRejection))
            Assert.NotNull(SsoErrorCode.FromDecision(decision));
    }

    #endregion
}
