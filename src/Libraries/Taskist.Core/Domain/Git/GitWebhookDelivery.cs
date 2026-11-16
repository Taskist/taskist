using Taskist.Core.Domain.Common;

namespace Taskist.Core.Domain.Git;

public class GitWebhookDelivery : BaseEntity
{
    public int ProjectRepositoryId { get; set; }

    public string DeliveryId { get; set; }

    public string EventName { get; set; }

    public int Status { get; set; }

    public DateTime ReceivedOn { get; set; }

    public DateTime? ProcessedOn { get; set; }

    public int Attempts { get; set; }

    public string? Error { get; set; }

    public string? Payload { get; set; }

    public virtual ProjectRepository ProjectRepository { get; set; }
}
