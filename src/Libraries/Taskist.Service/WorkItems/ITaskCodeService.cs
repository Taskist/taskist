using Taskist.Core.Dtos.Git;
using Taskist.Service.Git;

namespace Taskist.Service.WorkItems;

public interface ITaskCodeService
{
    Task<TaskCodeDto> GetTaskCodeAsync(int backlogId, bool refresh = false);

    Task ClearCacheAsync(int backlogId);

    Task<IList<int>> ResolveTaskIdsForMergeRequestAsync(GitMergeRequestInfo mergeRequest, GitRepositoryContext repository);

    Task<IList<int>> ResolveTaskIdsAsync(int taskistProjectId, string? title, string? description, string? sourceBranch,
        DateTime? mergeRequestCreatedAt, GitRepositoryContext? repository);
}
