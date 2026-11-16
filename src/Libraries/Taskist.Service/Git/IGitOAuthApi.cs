using Taskist.Core.Dtos.Git;

namespace Taskist.Service.Git;

public interface IGitOAuthApi
{
    bool OAuthConfigured { get; }

    string BuildAuthorizeUrl(string redirectUri, string state, string codeChallenge);

    Task<GitResult<GitOAuthTokenDto>> ExchangeCodeAsync(string code, string redirectUri, string codeVerifier);

    Task<GitResult<GitOAuthTokenDto>> RefreshTokenAsync(string refreshToken);

    Task RevokeTokenAsync(string token, bool oauth);
}
