namespace Taskist.Core.Enums;

public enum NotificationType
{
    ReviewerAssigned = 1,
    MergeRequestApproved,
    ChangesRequested,
    MergeRequestRejected,
    MergeRequestComment,
    MergeRequestConflict,
    MergeRequestClosed
}
