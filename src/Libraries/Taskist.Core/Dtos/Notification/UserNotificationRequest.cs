using Taskist.Core.Enums;

namespace Taskist.Core.Dtos.Notification;

public class UserNotificationRequest
{
    public List<int> UserIds { get; set; } = [];

    public NotificationType Type { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Message { get; set; }

    public string? Url { get; set; }

    public string? DedupeKey { get; set; }

    public int? DedupeHours { get; set; }

    public bool Email { get; set; }

    public int? ChatProjectId { get; set; }
}
