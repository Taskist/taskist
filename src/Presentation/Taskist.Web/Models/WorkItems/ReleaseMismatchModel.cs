namespace Taskist.Web.Models.WorkItems;

public class ReleaseMismatchModel
{
    public int TaskId { get; set; }

    public string Title { get; set; }

    public string Project { get; set; }

    public string Release { get; set; }

    public string Status { get; set; }
}
