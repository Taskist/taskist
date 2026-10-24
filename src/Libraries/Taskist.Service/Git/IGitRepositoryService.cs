using Taskist.Core.Domain.Git;
using Taskist.Core.Domain.WorkItems;

namespace Taskist.Service.Git;

public interface IGitRepositoryService
{
    Task<bool> AnyConfiguredAsync();

    Task<GitConnection?> GetConnectionByIdAsync(int id);

    Task<IList<GitConnection>> GetConnectionsAsync(bool activeOnly = true);

    Task<GitConnection?> GetDefaultConnectionAsync();

    Task InsertConnectionAsync(GitConnection connection);

    Task UpdateConnectionAsync(GitConnection connection);

    Task DeleteConnectionAsync(GitConnection connection);

    IGitProvider ProviderFor(GitConnection connection);

    Task<ProjectRepository?> GetRepositoryByIdAsync(int id);

    Task<ProjectRepository?> GetRepositoryByWebhookKeyAsync(string key);

    Task<IList<ProjectRepository>> GetProjectRepositoriesAsync(int projectId, bool includeInactive = false);

    Task<IList<ProjectRepository>> GetRepositoriesForProjectsAsync(IList<int> projectIds);

    Task<IList<ProjectRepository>> GetIssueSyncRepositoriesAsync();

    Task InsertRepositoryAsync(ProjectRepository repository);

    Task UpdateRepositoryAsync(ProjectRepository repository);

    Task DeleteRepositoryAsync(ProjectRepository repository);

    Task<GitRepositoryContext?> GetContextAsync(ProjectRepository? repository);

    Task<GitRepositoryContext?> GetContextAsync(int repositoryId);

    Task<GitRepositoryContext?> GetPrimaryContextAsync(int projectId);

    Task<IList<GitRepositoryContext>> GetProjectContextsAsync(int projectId);

    Task<GitRepositoryContext?> GetBranchContextAsync(GitBranch branch);

    Task<GitRepositoryContext?> GetClosingContextAsync(Backlog task);

    Task<GitRepositoryContext?> FindProjectContextAsync(int projectId, string? reference);

    string NewWebhookKey();

    string NewWebhookSecret();
}
