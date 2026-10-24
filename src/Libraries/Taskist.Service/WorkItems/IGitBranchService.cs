using Taskist.Core.Domain.Users;
using Taskist.Core.Domain.WorkItems;
using Taskist.Core.Dtos.Git;
using Taskist.Service.Git;

namespace Taskist.Service.WorkItems;

public interface IGitBranchService
{
    bool CreationEnabled { get; }

    Task<bool> CanManageBranchAsync(int backlogId);

    Task<BranchContextDto> GetContextAsync(IList<int> backlogIds, int? repositoryId = null);

    Task<BranchActionResult> CreateBranchAsync(IList<int> backlogIds, string branchName, string baseBranch, bool includeExisting = false, int? repositoryId = null);

    Task<BranchActionResult> LinkBranchAsync(IList<int> backlogIds, string branchName, int? repositoryId = null);

    Task<BranchActionResult> UnlinkBranchAsync(int backlogId, int gitBranchId);

    Task LinkClosingMergeRequestBranchAsync(int backlogId, int mergeRequestIid, int? repositoryId = null);

    Task<MergeRequestContextDto> GetMergeRequestContextAsync(int backlogId, int gitBranchId);

    Task<MergeRequestCreatedResult> CreateMergeRequestAsync(int backlogId, int gitBranchId, string title, string targetBranch,
        string? description, bool draft, bool removeSourceBranch);

    Task<(List<string>? Branches, string? Error)> SearchBranchesAsync(int backlogId, string? term, int? repositoryId = null);

    Task HandlePushAsync(GitPushInfo push, GitRepositoryContext repository, User? pusher);

    Task<IList<GitBranch>> GetUnlinkedBranchesAsync(IList<int> projectIds);

    Task<IList<int>> GetBranchCaretakerProjectIdsAsync(int userId);

    Task<Dictionary<int, List<string>>> GetActiveBranchNamesAsync(IList<int> backlogIds);
}
