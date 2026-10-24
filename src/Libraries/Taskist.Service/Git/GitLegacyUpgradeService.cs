using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Taskist.Core.Domain.Git;
using Taskist.Core.Domain.Masters;
using Taskist.Core.Domain.Settings;
using Taskist.Core.Enums;
using Taskist.Core.Security;
using Taskist.Data.Repository;
using Taskist.Service.Masters;

namespace Taskist.Service.Git;

public class GitLegacyUpgradeService : IGitLegacyUpgradeService
{
    #region Fields

    private readonly IRepository<GitConnection> _connectionRepository;
    private readonly IRepository<ProjectRepository> _projectRepositoryRepository;
    private readonly IRepository<Project> _projectRepository;
    private readonly IRepository<UserGitAccount> _accountRepository;
    private readonly IRepository<EmailAccount> _emailAccountRepository;
    private readonly IRepository<GenericAttribute> _genericAttributeRepository;
    private readonly IGitRepositoryService _gitRepositoryService;
    private readonly ISettingService _settingService;
    private readonly ISecretProtector _secretProtector;
    private readonly ILogger<GitLegacyUpgradeService> _logger;

    #endregion

    #region Ctor

    public GitLegacyUpgradeService(IRepository<GitConnection> connectionRepository,
        IRepository<ProjectRepository> projectRepositoryRepository,
        IRepository<Project> projectRepository,
        IRepository<UserGitAccount> accountRepository,
        IRepository<EmailAccount> emailAccountRepository,
        IRepository<GenericAttribute> genericAttributeRepository,
        IGitRepositoryService gitRepositoryService,
        ISettingService settingService,
        ISecretProtector secretProtector,
        ILogger<GitLegacyUpgradeService> logger)
    {
        _connectionRepository = connectionRepository;
        _projectRepositoryRepository = projectRepositoryRepository;
        _projectRepository = projectRepository;
        _accountRepository = accountRepository;
        _emailAccountRepository = emailAccountRepository;
        _genericAttributeRepository = genericAttributeRepository;
        _gitRepositoryService = gitRepositoryService;
        _settingService = settingService;
        _secretProtector = secretProtector;
        _logger = logger;
    }

    #endregion

    #region Methods

    public async Task<int> UpgradeAsync()
    {
        var settings = await _settingService.LoadSettingAsync<IntegrationSettings>();
        var legacyUrl = settings.GitLabBaseUrl?.Trim();
        if (string.IsNullOrWhiteSpace(legacyUrl))
            return 0;

        var created = 0;
        var connection = await _connectionRepository.Table
            .Where(x => !x.Deleted && x.ProviderType == (int)GitProviderType.GitLab)
            .OrderByDescending(x => x.IsDefault).ThenBy(x => x.Id)
            .FirstOrDefaultAsync();

        if (connection == null)
        {
            var apiMarker = legacyUrl.IndexOf("/api/v", StringComparison.OrdinalIgnoreCase);
            connection = new GitConnection
            {
                Name = "GitLab",
                ProviderType = (int)GitProviderType.GitLab,
                BaseUrl = apiMarker > 0 ? legacyUrl[..apiMarker] : legacyUrl.TrimEnd('/'),
                ApiUrl = apiMarker > 0 ? legacyUrl.TrimEnd('/') + "/" : null,
                AuthMode = (int)GitAuthMode.AccessToken,
                ServiceToken = _secretProtector.Unprotect(settings.GitLabAccessToken),
                IsDefault = !await _connectionRepository.Table.AnyAsync(x => !x.Deleted && x.IsDefault),
                Active = true
            };
            await _gitRepositoryService.InsertConnectionAsync(connection);
            created++;
        }

        var legacySecret = _secretProtector.Unprotect(settings.WebhookSecret);
        var projects = await _projectRepository.Table
            .Where(x => !x.Deleted && x.GitProjectReference != null && x.GitProjectReference != "")
            .ToListAsync();

        foreach (var project in projects)
        {
            var exists = await _projectRepositoryRepository.Table.AnyAsync(x => x.ProjectId == project.Id && !x.Deleted);
            if (exists)
                continue;

            await _gitRepositoryService.InsertRepositoryAsync(new ProjectRepository
            {
                ProjectId = project.Id,
                GitConnectionId = connection.Id,
                RepoRef = project.GitProjectReference!.Trim(),
                IsPrimary = true,
                WebhookKey = _gitRepositoryService.NewWebhookKey(),
                WebhookSecret = string.IsNullOrWhiteSpace(legacySecret) ? null : legacySecret,
                WebhookSecretCreatedOn = string.IsNullOrWhiteSpace(legacySecret) ? null : DateTime.UtcNow,
                IssueSyncMode = (int)IssueSyncMode.Off,
                IssueImportLimitPerHour = 50,
                Active = true
            });
            created++;
        }

        settings.GitLabBaseUrl = null;
        settings.GitLabAccessToken = null;
        settings.WebhookSecret = null;
        await _settingService.SaveSettingAsync(settings);

        _logger.LogInformation("Upgraded the legacy GitLab settings into Git connections ({Count} rows created). The old encrypted values were cleared.", created);
        return created;
    }

    public async Task<int> ReEncryptSecretsAsync()
    {
        var count = 0;

        foreach (var connection in await _connectionRepository.Table.ToListAsync())
        {
            await _connectionRepository.RewritePropertiesAsync(connection,
                nameof(GitConnection.ServiceToken), nameof(GitConnection.AppPrivateKey), nameof(GitConnection.OAuthClientSecret));
            count++;
        }

        foreach (var repository in await _projectRepositoryRepository.Table.Where(x => x.WebhookSecret != null).ToListAsync())
        {
            await _projectRepositoryRepository.RewritePropertiesAsync(repository, nameof(ProjectRepository.WebhookSecret));
            count++;
        }

        foreach (var account in await _accountRepository.Table.ToListAsync())
        {
            await _accountRepository.RewritePropertiesAsync(account, nameof(UserGitAccount.AccessToken), nameof(UserGitAccount.RefreshToken));
            count++;
        }

        foreach (var emailAccount in await _emailAccountRepository.Table.ToListAsync())
        {
            await _emailAccountRepository.RewritePropertiesAsync(emailAccount, nameof(EmailAccount.Password));
            count++;
        }

        var legacyTokens = await _genericAttributeRepository.Table
            .Where(x => x.Key == "GitLab.PersonalToken" && x.Value != null && x.Value.StartsWith("enc:"))
            .ToListAsync();
        foreach (var attribute in legacyTokens)
        {
            attribute.Value = _secretProtector.Protect(_secretProtector.Unprotect(attribute.Value)) ?? string.Empty;
            await _genericAttributeRepository.UpdateAsync(attribute);
            count++;
        }

        return count;
    }

    #endregion
}
