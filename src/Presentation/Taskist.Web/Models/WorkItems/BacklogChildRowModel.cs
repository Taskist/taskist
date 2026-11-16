namespace Taskist.Web.Models.WorkItems;

public class BacklogChildRowModel
{
    public int Id { get; set; }

    public string Title { get; set; }

    public string TaskTypeName { get; set; }

    public string TaskTypeIcon { get; set; }

    public string TaskTypeColor { get; set; }

    public string StatusName { get; set; }

    public string StatusColor { get; set; }

    public string AssigneeName { get; set; }

    public DateOnly? DueDate { get; set; }

    public int SubTaskCount { get; set; }
}
