namespace Taskist.Core.Dtos.Git;

public class TaskIssueDto
{
    public List<Link> Links { get; set; } = [];

    public List<Target> PublishTargets { get; set; } = [];

    public bool CanManage { get; set; }

    public class Link
    {
        public int Id { get; set; }

        public int RepositoryId { get; set; }

        public string RepositoryName { get; set; } = string.Empty;

        public string ProviderName { get; set; } = string.Empty;

        public int Number { get; set; }

        public string? Url { get; set; }

        public string? RemoteState { get; set; }

        public bool SyncEnabled { get; set; }

        public DateTime? LastSyncedOn { get; set; }

        public string? LastError { get; set; }

        public bool PublicRepository { get; set; }

        public bool CommentsSynced { get; set; }
    }

    public class Target
    {
        public int RepositoryId { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool PublicRepository { get; set; }
    }
}
