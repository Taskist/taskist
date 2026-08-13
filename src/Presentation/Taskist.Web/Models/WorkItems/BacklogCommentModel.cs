using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.WorkItems;

public class BacklogCommentModel : BasePageModel
{
    public BacklogCommentModel()
    {
        Comments = [];
    }

    public int BackLogId { get; set; }

    /// <summary>
    /// The signed-in user's id, so their own comments can be right-aligned like a chat.
    /// </summary>
    public int CurrentUserId { get; set; }

    public List<BacklogCommentGridModel> Comments { get; set; }
}

public class BacklogCommentGridModel
{
    public int CommentById { get; set; }

    public string CommentBy { get; set; }

    public DateTime CommentOn { get; set; }

    public string Comment { get; set; }

    public bool SystemComment { get; set; }
}
