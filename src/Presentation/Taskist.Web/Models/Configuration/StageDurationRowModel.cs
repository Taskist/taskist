namespace Taskist.Web.Models.Configuration;

public class StageDurationRowModel
{
    public int Id { get; set; }

    public string Name { get; set; }

    public int FromStatusId { get; set; }

    public int ToStatusId { get; set; }

    public int? ProjectId { get; set; }

    public bool ShowOnTask { get; set; }

    public bool Active { get; set; } = true;
}
