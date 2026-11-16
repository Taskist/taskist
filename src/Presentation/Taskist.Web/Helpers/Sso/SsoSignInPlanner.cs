namespace Taskist.Web.Helpers.Sso;

public static class SsoSignInPlanner
{
    #region Methods

    public static SsoSignInDecision Decide(ExternalIdentity identity,
        bool hasLinkedUser,
        bool hasUserWithEmail,
        bool emailUserLinkedToThisProvider,
        bool requireVerifiedEmail,
        bool autoProvision)
    {
        ArgumentNullException.ThrowIfNull(identity);

        if (string.IsNullOrWhiteSpace(identity.Subject) || string.IsNullOrWhiteSpace(identity.Issuer))
            return SsoSignInDecision.RejectMissingSubject;

        if (hasLinkedUser)
            return SsoSignInDecision.UseLinkedUser;

        if (string.IsNullOrWhiteSpace(identity.Email))
            return SsoSignInDecision.RejectMissingEmail;

        if (hasUserWithEmail)
        {
            if (requireVerifiedEmail && !identity.EmailVerified)
                return SsoSignInDecision.RejectUnverifiedEmail;

            if (emailUserLinkedToThisProvider)
                return SsoSignInDecision.RejectLinkConflict;

            return SsoSignInDecision.LinkExistingUser;
        }

        if (!autoProvision)
            return SsoSignInDecision.RejectNotProvisioned;

        if (requireVerifiedEmail && !identity.EmailVerified)
            return SsoSignInDecision.RejectUnverifiedEmail;

        return SsoSignInDecision.ProvisionUser;
    }

    public static bool IsRejection(SsoSignInDecision decision) =>
        decision is not (SsoSignInDecision.UseLinkedUser or SsoSignInDecision.LinkExistingUser or SsoSignInDecision.ProvisionUser);

    #endregion
}
