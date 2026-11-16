namespace Taskist.Core.Dtos.Git;

public class GitPushInfo
{
    public string? BranchName { get; set; }

    public bool IsNewBranch { get; set; }

    public bool IsDeletedBranch { get; set; }

    public string? PusherUserName { get; set; }

    public string? PusherName { get; set; }
}
