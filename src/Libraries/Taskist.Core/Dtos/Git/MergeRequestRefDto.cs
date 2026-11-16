namespace Taskist.Core.Dtos.Git;

public class MergeRequestRefDto
{
    public int Iid { get; set; }

    public string? Title { get; set; }

    public string? WebUrl { get; set; }

    public string? AuthorUserName { get; set; }

    public string? Sha { get; set; }

    public int ConnectionId { get; set; }

    public int RepositoryId { get; set; }

    public string RefPrefix { get; set; } = "!";
}
