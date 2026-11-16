using Taskist.Core.Domain.Common;

namespace Taskist.Core.Domain.Users;

public class UserNotification : BaseEntity
{
    public int UserId { get; set; }

    public int Type { get; set; }

    public string Title { get; set; }

    public string? Message { get; set; }

    public string? Url { get; set; }

    public string? DedupeKey { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime? ReadOnUtc { get; set; }

    public virtual User User { get; set; }
}
