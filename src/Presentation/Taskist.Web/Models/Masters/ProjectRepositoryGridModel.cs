namespace Taskist.Web.Models.Masters;

public class ProjectRepositoryGridModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Connection { get; set; } = string.Empty;

    public bool Primary { get; set; }

    public bool Active { get; set; }

    public string IssueSync { get; set; } = string.Empty;

    public bool WebhookSecured { get; set; }

    public string? LastDelivery { get; set; }
}
