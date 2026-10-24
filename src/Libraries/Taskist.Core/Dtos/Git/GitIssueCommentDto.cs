namespace Taskist.Core.Dtos.Git;

public class GitIssueCommentDto
{
    public string ExternalId { get; set; } = string.Empty;

    public string? Body { get; set; }

    public GitPersonDto? Author { get; set; }

    public bool System { get; set; }

    public string? Url { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
