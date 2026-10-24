namespace Taskist.Core.Dtos.Git;

public class BranchContextDto
{
    public string? Error { get; set; }

    public bool CanCreate { get; set; }

    public bool NeedsConnection { get; set; }

    public string? ConnectionMessage { get; set; }

    public string? SuggestedName { get; set; }

    public string? Prefix { get; set; }

    public string? DefaultBranch { get; set; }

    public List<string> BaseBranches { get; set; } = [];

    public List<int> EligibleTaskIds { get; set; } = [];

    public List<SkippedTask> Skipped { get; set; } = [];

    public List<SkippedTask> Existing { get; set; } = [];

    public int RepositoryId { get; set; }

    public List<RepositoryOption> Repositories { get; set; } = [];

    public class RepositoryOption
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class SkippedTask
    {
        public int TaskId { get; set; }

        public string Reason { get; set; }
    }
}
