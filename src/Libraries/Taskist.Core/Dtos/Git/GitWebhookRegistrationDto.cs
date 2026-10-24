namespace Taskist.Core.Dtos.Git;

public class GitWebhookRegistrationDto
{
    public string Url { get; set; } = string.Empty;

    public string Secret { get; set; } = string.Empty;

    public bool IssueEvents { get; set; }
}
