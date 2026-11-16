namespace Taskist.Core.Dtos.Git;

public class GitUserMergeRequestQuery
{
    public bool ReviewRequested { get; set; }

    public bool OpenOnly { get; set; }

    public string? UserName { get; set; }

    public int Limit { get; set; } = 50;
}
