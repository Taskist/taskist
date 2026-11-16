using Taskist.Core.Domain.Git;

namespace Taskist.Service.Git;

public class GitRepositoryContext
{
    public ProjectRepository Repository { get; init; }

    public GitConnection Connection { get; init; }

    public IGitProvider Provider { get; init; }

    public int RepositoryId => Repository.Id;

    public string RepoRef => Repository.RepoRef.Trim();

    public string Name => string.IsNullOrWhiteSpace(Repository.DisplayName) ? Repository.RepoRef : Repository.DisplayName!;

    public GitProviderCapabilities Capabilities => Provider.Capabilities;

    public bool Matches(string? reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
            return false;

        var value = reference.Trim().Trim('/');
        return string.Equals(value, RepoRef.Trim('/'), StringComparison.OrdinalIgnoreCase)
               || (!string.IsNullOrWhiteSpace(Repository.ExternalRepoId) && value == Repository.ExternalRepoId);
    }
}
