using Taskist.Core.Domain.Common;

namespace Taskist.Core.Domain.Logging;

public class AuditLog : BaseEntity
{
    public string EntityName { get; set; }

    public int? EntityId { get; set; }

    public string Action { get; set; }

    public int? UserId { get; set; }

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public string? IpAddress { get; set; }
}
