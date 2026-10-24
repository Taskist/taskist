namespace Taskist.Web.Models.Masters;

public class ProjectNotificationChannelGridModel
{
    public int Id { get; set; }

    public string Channel { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string Events { get; set; } = string.Empty;

    public bool Active { get; set; }
}
