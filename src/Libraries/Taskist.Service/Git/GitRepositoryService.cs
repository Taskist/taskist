using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Taskist.Core.Domain.Git;
using Taskist.Core.Domain.WorkItems;
using Taskist.Core.Enums;
using Taskist.Data.Repository;

namespace Taskist.Service.Git;

public class GitRepositoryService : IGitRepositoryService
{
    #region Fields

    private readonly IRepository<GitConnection> _connectionRepository;
    private readonly IRepository<ProjectRepository> _projectRepositoryRepository;
    private readonly IGitProviderFactory _providerFactory;
    private readonly IGitHubAppTokenService _gitHubAppTokenService;

    #endregion

    #region Ctor

    public GitRepositoryService(IRepository<GitConnection> connectionRepository,
        IRepository<ProjectRepository> projectRepositoryRepository,
        IGitProviderFactory providerFactory,
        IGitHubAppTokenService gitHubAppTokenService)
    {
        _connectionRepository = connectionRepository;
        _projectRepositoryRepository = projectRepositoryRepository;
        _providerFactory = providerFactory;
        _gitHubAppTokenService = gitHubAppTokenService;
    }

    #endregion

    #region Connections

    public async Task<bool> AnyConfiguredAsync()
    {
        return await _connectionRepository.Table.AnyAsync(x => x.Active && !x.Deleted);
    }

    public async Task<GitConnection?> GetConnectionByIdAsync(int id)
    {
        var connection = id > 0 ? await _connectionRepository.GetByIdAsync(id) : null;
        return connection == null || connection.Deleted ? null : connection;
    }

    public async Task<IList<GitConnection>> GetConnectionsAsync(bool activeOnly = true)
    {
        return await _connectionRepository.Table
            .Where(x => !x.Deleted && (!activeOnly || x.Active))
            .OrderByDescending(x => x.IsDefault).ThenBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<GitConnection?> GetDefaultConnectionAsync()
    {
        return await _connectionRepository.Table
            .Where(x => !x.Deleted && x.Active)
            .OrderByDescending(x => x.IsDefault).ThenBy(x => x.Id)
            .FirstOrDefaultAsync();
    }

    public async Task InsertConnectionAsync(GitConnection connection)
    {
        connection.CreatedOn = DateTime.UtcNow;
        await _connectionRepository.InsertAsync(connection);
        await EnsureSingleDefaultAsync(connection);
    }

    public async Task UpdateConnectionAsync(GitConnection connection)
    {
        connection.ModifiedOn = DateTime.UtcNow;
        await _connectionRepository.UpdateAsync(connection);
        _gitHubAppTokenService.Forget(connection.Id);
        await EnsureSingleDefaultAsync(connection);
    }

    public async Task DeleteConnectionAsync(GitConnection connection)
    {
        connection.Deleted = true;
        connection.Active = false;
        connection.IsDefault = false;
        connection.ModifiedOn = DateTime.UtcNow;
        await _connectionRepository.UpdateAsync(connection);
        _gitHubAppTokenService.Forget(connection.Id);
    }

    public IGitProvider ProviderFor(GitConnection connection) => _providerFactory.Create(connection);

    #endregion

    #region Repositories

    public async Task<ProjectRepository?> GetRepositoryByIdAsync(int id)
    {
        var repository = id > 0 ? await _projectRepositoryRepository.GetByIdAsync(id) : null;
        return repository == null || repository.Deleted ? null : repository;
    }

    public async Task<ProjectRepository?> GetRepositoryByWebhookKeyAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Length > 64)
            return null;

        return await _projectRepositoryRepository.Table
            .FirstOrDefaultAsync(x => x.WebhookKey == key && !x.Deleted && x.Active);
    }

    public async Task<IList<ProjectRepository>> GetProjectRepositoriesAsync(int projectId, bool includeInactive = false)
    {
        return await _projectRepositoryRepository.Table
            .Where(x => x.ProjectId == projectId && !x.Deleted && (includeInactive || (x.Active && x.GitConnection.Active && !x.GitConnection.Deleted)))
            .OrderByDescending(x => x.IsPrimary).ThenBy(x => x.Id)
            .ToListAsync();
    }

    public async Task<IList<ProjectRepository>> GetRepositoriesForProjectsAsync(IList<int> projectIds)
    {
        if (projectIds == null || projectIds.Count == 0)
            return [];

        return await _projectRepositoryRepository.Table
            .Where(x => projectIds.Contains(x.ProjectId) && !x.Deleted && x.Active && x.GitConnection.Active && !x.GitConnection.Deleted)
            .OrderBy(x => x.ProjectId).ThenByDescending(x => x.IsPrimary).ThenBy(x => x.Id)
            .ToListAsync();
    }

    public async Task<IList<ProjectRepository>> GetIssueSyncRepositoriesAsync()
    {
        return await _projectRepositoryRepository.Table
            .Where(x => !x.Deleted && x.Active && x.IssueSyncMode != (int)IssueSyncMode.Off
                        && x.GitConnection.Active && !x.GitConnection.Deleted
                        && x.Project.Active && !x.Project.Deleted)
            .ToListAsync();
    }

    public async Task InsertRepositoryAsync(ProjectRepository repository)
    {
        repository.CreatedOn = DateTime.UtcNow;
        if (string.IsNullOrWhiteSpace(repository.WebhookKey))
            repository.WebhookKey = NewWebhookKey();

        await _projectRepositoryRepository.InsertAsync(repository);
        await EnsureSinglePrimaryAsync(repository);
    }

    public async Task UpdateRepositoryAsync(ProjectRepository repository)
    {
        repository.ModifiedOn = DateTime.UtcNow;
        await _projectRepositoryRepository.UpdateAsync(repository);
        await EnsureSinglePrimaryAsync(repository);
    }

    public async Task DeleteRepositoryAsync(ProjectRepository repository)
    {
        repository.Deleted = true;
        repository.Active = false;
        repository.IsPrimary = false;
        repository.ModifiedOn = DateTime.UtcNow;
        await _projectRepositoryRepository.UpdateAsync(repository);

        var next = (await GetProjectRepositoriesAsync(repository.ProjectId)).FirstOrDefault();
        if (next != null && !next.IsPrimary)
        {
            next.IsPrimary = true;
            await _projectRepositoryRepository.UpdateAsync(next);
        }
    }

    #endregion

    #region Contexts

    public Task<GitRepositoryContext?> GetContextAsync(ProjectRepository? repository)
    {
        if (repository == null || repository.Deleted || !repository.Active)
            return Task.FromResult<GitRepositoryContext?>(null);

        var connection = repository.GitConnection;
        if (connection == null || connection.Deleted || !connection.Active)
            return Task.FromResult<GitRepositoryContext?>(null);

        return Task.FromResult<GitRepositoryContext?>(new GitRepositoryContext
        {
            Repository = repository,
            Connection = connection,
            Provider = _providerFactory.Create(connection)
        });
    }

    public async Task<GitRepositoryContext?> GetContextAsync(int repositoryId)
    {
        return await GetContextAsync(await GetRepositoryByIdAsync(repositoryId));
    }

    public async Task<GitRepositoryContext?> GetPrimaryContextAsync(int projectId)
    {
        return await GetContextAsync((await GetProjectRepositoriesAsync(projectId)).FirstOrDefault());
    }

    public async Task<IList<GitRepositoryContext>> GetProjectContextsAsync(int projectId)
    {
        var result = new List<GitRepositoryContext>();
        foreach (var repository in await GetProjectRepositoriesAsync(projectId))
        {
            var context = await GetContextAsync(repository);
            if (context != null)
                result.Add(context);
        }

        return result;
    }

    public async Task<GitRepositoryContext?> GetBranchContextAsync(GitBranch branch)
    {
        if (branch.ProjectRepositoryId > 0)
        {
            var direct = await GetContextAsync(branch.ProjectRepositoryId.Value);
            if (direct != null)
                return direct;
        }

        return await FindProjectContextAsync(branch.ProjectId, branch.GitProjectReference);
    }

    public async Task<GitRepositoryContext?> GetClosingContextAsync(Backlog task)
    {
        if (task.MergeRequestRepositoryId > 0)
        {
            var direct = await GetContextAsync(task.MergeRequestRepositoryId.Value);
            if (direct != null)
                return direct;
        }

        return await GetPrimaryContextAsync(task.ProjectId);
    }

    public async Task<GitRepositoryContext?> FindProjectContextAsync(int projectId, string? reference)
    {
        var contexts = await GetProjectContextsAsync(projectId);
        return contexts.FirstOrDefault(c => c.Matches(reference)) ?? (string.IsNullOrWhiteSpace(reference) ? contexts.FirstOrDefault() : null);
    }

    public string NewWebhookKey() => RandomToken(24);

    public string NewWebhookSecret() => RandomToken(32);

    #endregion

    #region Utilities

    private static string RandomToken(int bytes)
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(bytes)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private async Task EnsureSingleDefaultAsync(GitConnection connection)
    {
        if (!connection.IsDefault)
            return;

        var others = await _connectionRepository.Table.Where(x => x.Id != connection.Id && x.IsDefault).ToListAsync();
        foreach (var other in others)
            other.IsDefault = false;
        if (others.Count > 0)
            await _connectionRepository.UpdateAsync(others);
    }

    private async Task EnsureSinglePrimaryAsync(ProjectRepository repository)
    {
        var siblings = await _projectRepositoryRepository.Table
            .Where(x => x.ProjectId == repository.ProjectId && x.Id != repository.Id && !x.Deleted)
            .ToListAsync();

        if (repository.IsPrimary)
        {
            var demoted = siblings.Where(x => x.IsPrimary).ToList();
            foreach (var other in demoted)
                other.IsPrimary = false;
            if (demoted.Count > 0)
                await _projectRepositoryRepository.UpdateAsync(demoted);
        }
        else if (!siblings.Any(x => x.IsPrimary))
        {
            repository.IsPrimary = true;
            await _projectRepositoryRepository.UpdateAsync(repository);
        }
    }

    #endregion
}
