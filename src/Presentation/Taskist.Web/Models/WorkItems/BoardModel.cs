using Microsoft.AspNetCore.Mvc.Rendering;

namespace Taskist.Web.Models.WorkItems;

public class BoardModel
{
    public int ProjectId { get; set; }

    public List<SelectListItem> Projects { get; set; } = [];

    public List<BoardColumnModel> Columns { get; set; } = [];

    public bool HasProject => ProjectId > 0;
}
