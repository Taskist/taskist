namespace Taskist.Web.Models.WorkItems;

public class BoardCardModel
{
    public int Id { get; set; }

    public string Title { get; set; }

    public int TaskTypeId { get; set; }

    public string StatusColor { get; set; }

    public string TaskTypeName { get; set; }

    public string TypeColor { get; set; }

    public string TypeTextColor { get; set; }

    public string TypeIcon { get; set; }

    public string SeverityName { get; set; }

    public string SeverityColor { get; set; }

    public string SeverityTextColor { get; set; }

    public string SeverityIcon { get; set; }

    public string AssigneeName { get; set; }

    public DateOnly? DueDate { get; set; }

    public int CommentCount { get; set; }

    public int AttachmentCount { get; set; }

    public int SubTaskCount { get; set; }
}
