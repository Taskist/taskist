namespace Taskist.Core.Dtos.Git;

public class BranchCleanupResult
{
    public int Deleted { get; set; }

    public int AlreadyGone { get; set; }

    public List<string> Failures { get; set; } = [];

    public bool NeedsConnection { get; set; }
}
