namespace Taskist.Web.Models.WorkItems;

public class BacklogCommentGridModel
{
    public int CommentById { get; set; }

    public string CommentBy { get; set; }

    public DateTime CommentOn { get; set; }

    public string Comment { get; set; }

    public bool SystemComment { get; set; }

    public string? FieldLabel { get; set; }

    public string? ChangeAction { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public bool IsTextChange => !string.IsNullOrEmpty(FieldLabel);
}
