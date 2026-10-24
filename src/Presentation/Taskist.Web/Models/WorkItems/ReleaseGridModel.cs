using Taskist.Core.Dtos.WorkItems;
using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.WorkItems;

public class ReleaseGridModel : BaseModel
{
    public string Name { get; set; }

    public string ReleaseType { get; set; }

    public string? Description { get; set; }

    public string Project { get; set; }

    public IList<string> ProjectNames { get; set; } = [];

    public DateOnly ReleaseDate { get; set; }

    public string Status { get; set; }

    public int TotalBacklog { get; set; }

    public int CompletedBacklog { get; set; }

    public IList<ReleaseProjectProgressDto> ProjectProgress { get; set; } = [];
}
