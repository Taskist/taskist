namespace Taskist.Core.Dtos.Git;

public class BranchCleanupDto
{
    public List<BranchCleanupItemDto> Items { get; set; } = [];

    public List<string> FailedProjects { get; set; } = [];

    public string? Error { get; set; }

    public int IdleDays { get; set; }

    public bool Connected { get; set; }
}
