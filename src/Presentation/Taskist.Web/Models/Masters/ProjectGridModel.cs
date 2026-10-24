using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.Masters;

public class ProjectGridModel : BaseModel
{
    public string Name { get; set; }

    public string? Description { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string ClientName { get; set; }

    public bool Active { get; set; }
}
