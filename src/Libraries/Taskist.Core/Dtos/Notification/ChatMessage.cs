using Taskist.Core.Enums;

namespace Taskist.Core.Dtos.Notification;

public class ChatMessage
{
    public ChatEvent Event { get; set; }

    public string? ProjectName { get; set; }

    public string Title { get; set; } = string.Empty;

    public List<string> Lines { get; set; } = [];

    public string? Url { get; set; }

    public List<ChatRecipient> Recipients { get; set; } = [];
}
