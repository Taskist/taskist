using Taskist.Core.Enums;

namespace Taskist.Core.Dtos.Notification;

public class ChatTaskUpdateDto
{
    public string Name { get; set; }

    public string Email { get; set; }

    public int TaskId { get; set; }

    public string? TaskTitle { get; set; }

    public string Message { get; set; }

    public int ProjectId { get; set; }

    public ChatEvent Event { get; set; }
}