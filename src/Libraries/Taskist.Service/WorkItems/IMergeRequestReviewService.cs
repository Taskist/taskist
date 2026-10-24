using Taskist.Core.Domain.Users;
using Taskist.Core.Dtos.Git;
using Taskist.Service.Git;

namespace Taskist.Service.WorkItems;

public interface IMergeRequestReviewService
{
    Task<IList<ReviewProjectScopeDto>> GetScopeAsync(User user);

    Task<ReviewProjectScopeDto?> GetRepositoryScopeAsync(User user, int repositoryId);

    Task<bool> CanReviewProjectAsync(User user, int projectId);

    Task<ReviewQueueDto> GetQueueAsync(User user, bool refresh = false);

    Task<(ReviewDetailDto? Detail, string? Error)> GetDetailAsync(User user, int repositoryId, int iid);

    Task<(MergeRequestChangesDto? Changes, string? Error)> GetChangesAsync(User user, int repositoryId, int iid);

    Task<ReviewActionResult> SetApprovalAsync(User user, int repositoryId, int iid, bool approve);

    Task<ReviewActionResult> CommentAsync(User user, int repositoryId, int iid, string body);

    Task<ReviewActionResult> RejectAsync(User user, int repositoryId, int iid, string reason);

    Task<ReviewActionResult> RequestChangesAsync(User user, int repositoryId, int iid, string notes);

    Task<ReviewActionResult> MergeAsync(User user, int repositoryId, int iid, string? sha, bool squash, bool removeSourceBranch);

    Task<ReviewActionResult> RebaseAsync(User user, int repositoryId, int iid);

    Task AutoAssignReviewerAsync(GitRepositoryContext repository, int iid, long authorExternalId);

    Task ClearQueueCacheAsync(int repositoryId);

    Task<(int Conflicting, int Notified)> CheckConflictsAsync(User user);
}
