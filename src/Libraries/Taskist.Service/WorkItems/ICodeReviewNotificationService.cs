using Taskist.Core.Dtos.Git;

namespace Taskist.Service.WorkItems;

public interface ICodeReviewNotificationService
{
    Task ReviewerAssignedAsync(int projectId, MergeRequestRefDto mergeRequest, IEnumerable<string?> reviewerUserNames, string? actorUserName);

    Task ApprovedAsync(int projectId, MergeRequestRefDto mergeRequest, string? approverUserName, string approverName);

    Task ChangesRequestedAsync(int projectId, MergeRequestRefDto mergeRequest, string? reviewerUserName, string reviewerName, string notes);

    Task RejectedAsync(int projectId, MergeRequestRefDto mergeRequest, string? actorUserName, string actorName, string reason);

    Task ClosedAsync(int projectId, MergeRequestRefDto mergeRequest, string? actorUserName, string? actorName);

    Task CommentedAsync(int projectId, MergeRequestRefDto mergeRequest, string? commenterUserName, string? commenterName, string? note, string? noteUrl);

    Task<bool> ConflictAsync(int projectId, string projectName, MergeRequestRefDto mergeRequest);
}
