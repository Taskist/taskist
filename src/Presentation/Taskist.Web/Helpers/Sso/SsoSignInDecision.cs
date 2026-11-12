namespace Taskist.Web.Helpers.Sso;

public enum SsoSignInDecision
{
    UseLinkedUser,
    LinkExistingUser,
    ProvisionUser,
    RejectMissingSubject,
    RejectMissingEmail,
    RejectUnverifiedEmail,
    RejectLinkConflict,
    RejectNotProvisioned
}
