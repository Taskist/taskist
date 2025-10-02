using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.WorkItems;

public class BacklogCommentModel : BaseModel
{
    public BacklogCommentModel()
    {
        Comments = [];
    }

    public int BackLogId { get; set; }

    public List<BacklogCommentGridModel> Comments { get; set; }
}

public class BacklogCommentGridModel
{
    public string CommentBy { get; set; }

    public DateTime CommentOn { get; set; }

    public string Comment { get; set; }
}
