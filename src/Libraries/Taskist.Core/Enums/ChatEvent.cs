namespace Taskist.Core.Enums;

[Flags]
public enum ChatEvent
{
    None = 0,
    TaskAssigned = 1,
    StatusChanged = 2,
    TaskUpdated = 4,
    Commented = 8,
    MergeRequestOpened = 16,
    MergeRequestMerged = 32,
    MergeRequestClosed = 64,
    CodeReview = 128,
    Conflict = 256,
    All = TaskAssigned | StatusChanged | TaskUpdated | Commented | MergeRequestOpened | MergeRequestMerged | MergeRequestClosed | CodeReview | Conflict
}
