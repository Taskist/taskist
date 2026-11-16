using Taskist.Core.Domain.Git;
using Taskist.Core.Enums;
using Taskist.Service.Git.Providers;
using Taskist.Service.Git.Providers.GitHub;
using Taskist.Service.Git.Providers.GitLab;

namespace Taskist.Service.Git;

public class GitProviderFactory : IGitProviderFactory
{
    #region Fields

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IGitHubAppTokenService _gitHubAppTokenService;
    private readonly Dictionary<string, IGitProvider> _providers = new();

    #endregion

    #region Ctor

    public GitProviderFactory(IHttpClientFactory httpClientFactory,
        IGitHubAppTokenService gitHubAppTokenService)
    {
        _httpClientFactory = httpClientFactory;
        _gitHubAppTokenService = gitHubAppTokenService;
    }

    #endregion

    #region Methods

    public IGitProvider Create(GitConnection connection)
    {
        var key = $"{connection.Id}:{connection.ModifiedOn?.Ticks ?? 0}:{connection.ProviderType}:{connection.BaseUrl}";
        if (connection.Id > 0 && _providers.TryGetValue(key, out var cached))
            return cached;

        var webBase = connection.BaseUrl.Trim().TrimEnd('/');
        var apiUrl = string.IsNullOrWhiteSpace(connection.ApiUrl) ? DefaultApiUrl(connection.ProviderType, webBase) : connection.ApiUrl.Trim();
        if (!apiUrl.EndsWith('/'))
            apiUrl += "/";

        var context = new GitProviderContext
        {
            Connection = connection,
            Http = _httpClientFactory.CreateClient(connection.AllowInsecureHttp ? GitHttp.LocalClientName : GitHttp.ClientName),
            ApiBase = new Uri(apiUrl),
            WebBase = webBase,
            ServiceToken = connection.ServiceToken,
            AppPrivateKey = connection.AppPrivateKey,
            OAuthClientSecret = connection.OAuthClientSecret,
            AppTokens = _gitHubAppTokenService
        };

        IGitProvider provider = connection.ProviderType == (int)GitProviderType.GitHub
            ? new GitHubProvider(context)
            : new GitLabProvider(context);

        if (connection.Id > 0)
            _providers[key] = provider;

        return provider;
    }

    public IGitWebhookParser CreateWebhookParser(int providerType)
    {
        return providerType == (int)GitProviderType.GitHub ? new GitHubWebhookParser() : new GitLabWebhookParser();
    }

    public string DefaultApiUrl(int providerType, string baseUrl)
    {
        var web = baseUrl.Trim().TrimEnd('/');
        if (providerType == (int)GitProviderType.GitHub)
            return string.Equals(web, "https://github.com", StringComparison.OrdinalIgnoreCase) ? "https://api.github.com/" : $"{web}/api/v3/";

        return $"{web}/api/v4/";
    }

    #endregion
}
