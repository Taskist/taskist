namespace Taskist.Web.Models.WorkItems;

public class ReleaseMismatchGroupModel
{
    public int ReleaseId { get; set; }

    public string Release { get; set; }

    public string MappedProjects { get; set; }

    public int ProjectId { get; set; }

    public string Project { get; set; }

    public bool CanAddProject { get; set; }

    public IList<ReleaseMismatchModel> Tasks { get; set; } = [];
}
