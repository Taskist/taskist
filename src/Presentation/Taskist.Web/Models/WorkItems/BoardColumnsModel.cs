namespace Taskist.Web.Models.WorkItems;

public class BoardColumnsModel
{
    public int ProjectId { get; set; }

    public List<BoardColumnOptionModel> Options { get; set; } = [];
}
