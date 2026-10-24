using Microsoft.AspNetCore.Mvc.Rendering;

namespace Taskist.Web.Models.WorkItems;

public class TimelineModel
{
    public int ProjectId { get; set; }

    public List<SelectListItem> Projects { get; set; } = [];

    public bool HasProject => ProjectId > 0;
}
