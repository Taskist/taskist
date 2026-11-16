using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.WorkItems;

public class BacklogCommentModel : BaseModel
{
    public BacklogCommentModel()
    {
        Comments = [];
    }

    public int BackLogId { get; set; }

    public bool CanPostToIssue { get; set; }

    public List<BacklogCommentGridModel> Comments { get; set; }
}
