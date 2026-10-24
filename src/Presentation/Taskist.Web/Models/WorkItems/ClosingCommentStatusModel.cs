namespace Taskist.Web.Models.WorkItems;

public class ClosingCommentStatusModel
{
    public bool Valid { get; set; }

    public bool RootCauseMissing { get; set; }

    public bool ResolutionMissing { get; set; }

    public bool MrMissing { get; set; }
}
