using Microsoft.EntityFrameworkCore;
using Taskist.Core.Domain.Git;
using Taskist.Core.Domain.Users;
using Taskist.Core.Dtos.Git;
using Taskist.Core.Enums;
using Taskist.Core.Security;
using Taskist.Data.Repository;
using Taskist.Service.Masters;

namespace Taskist.Service.Git;

public class GitAccountService : IGitAccountService
{
    #region Fields

    private const string LegacyTokenKey = "GitLab.PersonalToken";
    private const string LegacyUserNameKey = "GitLab.PersonalUserName";
    private const string LegacyExpiresKey = "GitLab.PersonalTokenExpires";
    private const string LegacyConnectedKey = "GitLab.PersonalConnectedOn";

    private readonly IRepository<UserGitAccount> _accountRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IGitRepositoryService _gitRepositoryService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly ISecretProtector _secretProtector;

    #endregion

    #region Ctor

    public GitAccountService(IRepository<UserGitAccount> accountRepository,
        IRepository<User> userRepository,
        IGitRepositoryService gitRepositoryService,
        IGenericAttributeService genericAttributeService,
        ISecretProtector secretProtector)
    {
        _accountRepository = accountRepository;
        _userRepository = userRepository;
        _gitRepositoryService = gitRepositoryService;
        _genericAttributeService = genericAttributeService;
        _secretProtector = secretProtector;
    }

    #endregion

    #region Methods

    public async Task<IList<GitAccountStatusDto>> GetStatusesAsync(User user)
    {
        var result = new List<GitAccountStatusDto>();
        foreach (var connection in await _gitRepositoryService.GetConnectionsAsync())
            result.Add(await GetStatusAsync(user, connection));

        return result;
    }

    public async Task<GitAccountStatusDto> GetStatusAsync(User user, GitConnection connection)
    {
        var provider = _gitRepositoryService.ProviderFor(connection);
        var account = await GetAccountAsync(user, connection);
        var web = connection.BaseUrl.Trim().TrimEnd('/');

        return new GitAccountStatusDto
        {
            ConnectionId = connection.Id,
            ConnectionName = connection.Name,
            ProviderType = connection.ProviderType,
            ProviderName = provider.Capabilities.Name,
            WebUrl = web,
            Connected = account != null,
            OAuthAvailable = provider.OAuthConfigured,
            IsOAuth = account?.TokenType == (int)GitAccountTokenType.OAuth,
            AccountUserName = account?.UserName,
            ExpectedUserName = connection.IsDefault ? user.GitUserName : null,
            Scopes = account?.Scopes,
            ExpiresOn = account?.ExpiresOn,
            ConnectedOn = account?.ConnectedOn,
            RequiredScopes = provider.Capabilities.RequiredUserScopes,
            TokenHelpUrl = connection.ProviderType == (int)GitProviderType.GitHub
                ? $"{web}/settings/tokens/new?scopes=repo&description=Taskist"
                : $"{web}/-/user_settings/personal_access_tokens?name=Taskist&scopes=api"
        };
    }

    public async Task<string?> GetTokenAsync(User? user, GitConnection connection)
    {
        if (user == null)
            return null;

        var account = await GetAccountAsync(user, connection);
        if (account == null || string.IsNullOrWhiteSpace(account.AccessToken))
            return null;

        if (account.TokenType == (int)GitAccountTokenType.OAuth && account.ExpiresOn != null && account.ExpiresOn < DateTime.UtcNow.AddMinutes(2))
        {
            if (string.IsNullOrWhiteSpace(account.RefreshToken))
                return null;

            var refreshed = await _gitRepositoryService.ProviderFor(connection).RefreshTokenAsync(account.RefreshToken);
            if (!refreshed.Success || refreshed.Data == null)
                return null;

            account.AccessToken = refreshed.Data.AccessToken;
            account.RefreshToken = refreshed.Data.RefreshToken ?? account.RefreshToken;
            account.ExpiresOn = refreshed.Data.ExpiresOn;
            account.RefreshExpiresOn = refreshed.Data.RefreshExpiresOn ?? account.RefreshExpiresOn;
            await _accountRepository.UpdateAsync(account);
        }

        return account.AccessToken;
    }

    public async Task<(string? Token, string? Error)> RequireTokenAsync(User? user, GitConnection connection, string action)
    {
        var providerName = _gitRepositoryService.ProviderFor(connection).Capabilities.Name;
        if (user == null)
            return (null, $"Sign in again to {action}.");

        var status = await GetStatusAsync(user, connection);
        if (!status.Connected)
            return (null, $"You {action} in your own name, so connect your {providerName} account first (My profile → Git connections → {connection.Name}).");
        if (status.Expired)
            return (null, $"Your {providerName} token has expired. Create a new one and reconnect under My profile → Git connections.");

        var token = await GetTokenAsync(user, connection);
        return string.IsNullOrWhiteSpace(token)
            ? (null, $"Your saved {providerName} credentials could not be used. Reconnect under My profile → Git connections.")
            : (token, null);
    }

    public async Task<(bool Success, string Message)> ConnectWithTokenAsync(User user, int connectionId, string token)
    {
        token = token?.Trim() ?? string.Empty;
        if (token.Length == 0)
            return (false, "Paste your personal access token.");
        if (token.Length > 1000)
            return (false, "That does not look like a personal access token.");

        var connection = await _gitRepositoryService.GetConnectionByIdAsync(connectionId);
        if (connection == null || !connection.Active)
            return (false, "This Git connection is not available.");

        return await SaveAccountAsync(user, connection, token, GitAccountTokenType.Personal, null);
    }

    public async Task<(bool Success, string Message)> ConnectWithOAuthAsync(User user, int connectionId, GitOAuthTokenDto token)
    {
        var connection = await _gitRepositoryService.GetConnectionByIdAsync(connectionId);
        if (connection == null || !connection.Active)
            return (false, "This Git connection is not available.");

        return await SaveAccountAsync(user, connection, token.AccessToken, GitAccountTokenType.OAuth, token);
    }

    public async Task DisconnectAsync(User user, int connectionId)
    {
        var connection = await _gitRepositoryService.GetConnectionByIdAsync(connectionId);
        var account = await _accountRepository.Table.FirstOrDefaultAsync(x => x.UserId == user.Id && x.GitConnectionId == connectionId);

        if (account != null)
        {
            if (connection != null && !string.IsNullOrWhiteSpace(account.AccessToken))
            {
                try
                {
                    await _gitRepositoryService.ProviderFor(connection).RevokeTokenAsync(account.AccessToken, account.TokenType == (int)GitAccountTokenType.OAuth);
                }
                catch (Exception)
                {
                }
            }

            await _accountRepository.DeleteAsync(account);
        }

        if (connection?.IsDefault == true)
            await ClearLegacyAsync(user);
    }

    public async Task<string?> GetUserNameAsync(User? user, GitConnection connection)
    {
        if (user == null)
            return null;

        var account = await _accountRepository.Table.FirstOrDefaultAsync(x => x.UserId == user.Id && x.GitConnectionId == connection.Id);
        if (account != null)
            return account.UserName;

        return connection.IsDefault && !string.IsNullOrWhiteSpace(user.GitUserName) ? user.GitUserName.Trim() : null;
    }

    public async Task<User?> FindUserAsync(GitConnection connection, string? userName)
    {
        var name = userName?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var userId = await _accountRepository.Table
            .Where(x => x.GitConnectionId == connection.Id && x.UserName == name)
            .Select(x => (int?)x.UserId)
            .FirstOrDefaultAsync();

        if (userId > 0)
        {
            var linked = await _userRepository.GetByIdAsync(userId.Value);
            if (linked != null && !linked.Deleted)
                return linked;
        }

        if (!connection.IsDefault)
            return null;

        return await _userRepository.Table.FirstOrDefaultAsync(x => !x.Deleted && x.GitUserName == name);
    }

    public async Task<IList<int>> GetConnectedConnectionIdsAsync(int userId)
    {
        return await _accountRepository.Table.Where(x => x.UserId == userId).Select(x => x.GitConnectionId).ToListAsync();
    }

    #endregion

    #region Utilities

    private async Task<UserGitAccount?> GetAccountAsync(User user, GitConnection connection)
    {
        var account = await _accountRepository.Table.FirstOrDefaultAsync(x => x.UserId == user.Id && x.GitConnectionId == connection.Id);
        if (account != null || !connection.IsDefault || connection.ProviderType != (int)GitProviderType.GitLab)
            return account;

        return await MigrateLegacyAsync(user, connection);
    }

    private async Task<UserGitAccount?> MigrateLegacyAsync(User user, GitConnection connection)
    {
        var stored = await _genericAttributeService.GetAttributeAsync<string>(user, LegacyTokenKey);
        if (string.IsNullOrWhiteSpace(stored))
            return null;

        var token = _secretProtector.Unprotect(stored);
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var result = await SaveAccountAsync(user, connection, token, GitAccountTokenType.Personal, null);
        if (!result.Success)
            return null;

        return await _accountRepository.Table.FirstOrDefaultAsync(x => x.UserId == user.Id && x.GitConnectionId == connection.Id);
    }

    private async Task ClearLegacyAsync(User user)
    {
        foreach (var key in new[] { LegacyTokenKey, LegacyUserNameKey, LegacyExpiresKey, LegacyConnectedKey })
        {
            if (!string.IsNullOrEmpty(await _genericAttributeService.GetAttributeAsync<string>(user, key)))
                await _genericAttributeService.SaveAttributeAsync(user, key, string.Empty);
        }
    }

    private async Task<(bool Success, string Message)> SaveAccountAsync(User user, GitConnection connection, string token, GitAccountTokenType type, GitOAuthTokenDto? oauth)
    {
        var provider = _gitRepositoryService.ProviderFor(connection);
        var validated = await provider.ValidateUserTokenAsync(token);
        if (!validated.Success || validated.Data == null)
            return (false, validated.Error ?? $"{provider.Capabilities.Name} did not accept this token.");

        var info = validated.Data;
        if (!string.IsNullOrWhiteSpace(info.Problem))
            return (false, info.Problem);

        if (string.IsNullOrWhiteSpace(info.ExternalUserId) || string.IsNullOrWhiteSpace(info.UserName))
            return (false, $"{provider.Capabilities.Name} did not say which user owns this token.");

        if (connection.IsDefault && !string.IsNullOrWhiteSpace(user.GitUserName)
            && !string.Equals(info.UserName, user.GitUserName.Trim(), StringComparison.OrdinalIgnoreCase))
            return (false, $"This token belongs to {provider.Capabilities.Name} user '{info.UserName}', but your user record is linked to '{user.GitUserName}'. Create the token while signed in as yourself.");

        var takenBy = await _accountRepository.Table
            .Where(x => x.GitConnectionId == connection.Id && x.ExternalUserId == info.ExternalUserId && x.UserId != user.Id)
            .Select(x => (int?)x.UserId)
            .FirstOrDefaultAsync();
        if (takenBy > 0)
            return (false, $"The {provider.Capabilities.Name} account '{info.UserName}' is already connected to another Taskist user.");

        var account = await _accountRepository.Table.FirstOrDefaultAsync(x => x.UserId == user.Id && x.GitConnectionId == connection.Id);
        var isNew = account == null;
        account ??= new UserGitAccount { UserId = user.Id, GitConnectionId = connection.Id };

        account.ExternalUserId = info.ExternalUserId;
        account.UserName = info.UserName;
        account.DisplayName = info.DisplayName;
        account.TokenType = (int)type;
        account.AccessToken = token;
        account.RefreshToken = oauth?.RefreshToken;
        account.Scopes = oauth?.Scopes ?? (info.Scopes.Count > 0 ? string.Join(",", info.Scopes) : null);
        account.ExpiresOn = oauth?.ExpiresOn ?? info.ExpiresAt;
        account.RefreshExpiresOn = oauth?.RefreshExpiresOn;
        account.ConnectedOn = DateTime.UtcNow;

        if (isNew)
            await _accountRepository.InsertAsync(account);
        else
            await _accountRepository.UpdateAsync(account);

        if (connection.IsDefault)
            await ClearLegacyAsync(user);

        return (true, $"Connected as {info.UserName}.");
    }

    #endregion
}
