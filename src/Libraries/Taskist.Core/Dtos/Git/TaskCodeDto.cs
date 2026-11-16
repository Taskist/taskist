namespace Taskist.Core.Dtos.Git;

public class TaskCodeDto
{
    public string State { get; set; } = TaskCodeState.Ok;

    public bool Partial { get; set; }

    public string? ProjectWebUrl { get; set; }

    public string? ErrorMessage { get; set; }

    public bool MultipleRepositories { get; set; }

    public string ShortTerm { get; set; } = "MR";

    public string Term { get; set; } = "merge request";

    public List<Branch> Branches { get; set; } = [];

    public List<MergeRequest> MergeRequests { get; set; } = [];

    public DateTime LoadedOn { get; set; } = DateTime.UtcNow;

    public class Branch
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string? BaseBranch { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }

        public List<int> SharedWithTaskIds { get; set; } = [];

        public bool Deleted { get; set; }

        public int? Origin { get; set; }

        public int RepositoryId { get; set; }

        public string? RepositoryName { get; set; }

        public string? Url { get; set; }
    }

    public class MergeRequest
    {
        public int Iid { get; set; }

        public string Title { get; set; }

        public string State { get; set; }

        public bool Draft { get; set; }

        public string? SourceBranch { get; set; }

        public string? TargetBranch { get; set; }

        public string? Author { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string? Url { get; set; }

        public int RepositoryId { get; set; }

        public string? RepositoryName { get; set; }

        public string RefPrefix { get; set; } = "!";
    }
}
