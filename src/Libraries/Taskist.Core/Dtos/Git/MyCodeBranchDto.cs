namespace Taskist.Core.Dtos.Git;

public class MyCodeBranchDto
{
    public int BranchId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ProjectName { get; set; } = string.Empty;

    public int TaskId { get; set; }

    public int TaskCount { get; set; }

    public DateTime CreatedOn { get; set; }
}
