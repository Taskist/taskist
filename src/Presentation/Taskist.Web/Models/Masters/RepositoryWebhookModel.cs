namespace Taskist.Web.Models.Masters;

public class RepositoryWebhookModel
{
    public int RepositoryId { get; set; }

    public string RepositoryName { get; set; } = string.Empty;

    public int ProviderType { get; set; }

    public string WebhookUrl { get; set; } = string.Empty;

    public bool HasSecret { get; set; }

    public DateTime? SecretCreatedOn { get; set; }

    public DateTime? LastDeliveryOn { get; set; }

    public bool IssueEvents { get; set; }

    public string? LegacyUrl { get; set; }
}
