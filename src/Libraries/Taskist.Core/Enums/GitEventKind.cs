namespace Taskist.Core.Enums;

public enum GitEventKind
{
    Unknown = 0,
    Ping = 1,
    Push = 2,
    MergeRequest = 3,
    MergeRequestNote = 4,
    Issue = 5,
    IssueComment = 6
}
