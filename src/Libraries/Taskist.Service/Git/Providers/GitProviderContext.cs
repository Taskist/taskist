using Taskist.Core.Domain.Git;

namespace Taskist.Service.Git.Providers;

public class GitProviderContext
{
    public GitConnection Connection { get; init; }

    public HttpClient Http { get; init; }

    public Uri ApiBase { get; init; }

    public string WebBase { get; init; } = string.Empty;

    public string? ServiceToken { get; init; }

    public string? AppPrivateKey { get; init; }

    public string? OAuthClientSecret { get; init; }

    public IGitHubAppTokenService? AppTokens { get; init; }
}
