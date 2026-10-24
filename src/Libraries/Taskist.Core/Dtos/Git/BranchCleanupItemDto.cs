using Taskist.Core.Enums;

namespace Taskist.Core.Dtos.Git;

public class BranchCleanupItemDto
{
    public int BranchId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ProjectName { get; set; } = string.Empty;

    public BranchCleanupReason Reason { get; set; }

    public DateTime? LastCommitOn { get; set; }

    public string? CreatedBy { get; set; }

    public List<int> TaskIds { get; set; } = [];

    public string? WebUrl { get; set; }
}
