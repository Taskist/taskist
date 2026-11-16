namespace Taskist.Core.Dtos.Git;

public class GitIssueDto
{
    public string ExternalId { get; set; } = string.Empty;

    public int Number { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Body { get; set; }

    public bool Closed { get; set; }

    public List<string> Labels { get; set; } = [];

    public List<GitPersonDto> Assignees { get; set; } = [];

    public GitPersonDto? Author { get; set; }

    public string? Url { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
