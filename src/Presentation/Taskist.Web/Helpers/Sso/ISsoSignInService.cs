namespace Taskist.Web.Helpers.Sso;

public interface ISsoSignInService
{
    Task<SsoSignInResult> ResolveAsync(ExternalIdentity identity);
}
