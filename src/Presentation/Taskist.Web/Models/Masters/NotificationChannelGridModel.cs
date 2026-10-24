namespace Taskist.Web.Models.Masters;

public class NotificationChannelGridModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string? LastSent { get; set; }

    public string? LastError { get; set; }

    public bool Active { get; set; }
}
