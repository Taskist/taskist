namespace Taskist.Web.Models.WorkItems;

public class BacklogPageModel
{
    public int ProjectId { get; set; }

    public bool CanReport { get; set; }

    public bool CanEdit { get; set; }

    public bool CanClose { get; set; }

    public bool CanReOpen { get; set; }

    public bool CanComment { get; set; }

    public bool CanViewOthersTask { get; set; }

    public bool CanEditOthersTask { get; set; }

    public int FilterMode { get; set; }
}
