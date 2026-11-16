using Taskist.Core.Domain.Common;
using Taskist.Core.Domain.Masters;

namespace Taskist.Core.Domain.WorkItems;

public class Release : BaseEntity, ISoftDeletedEntity, IAuditableEntity
{
    public string Name { get; set; }

    public int ReleaseType { get; set; }

    public string? Description { get; set; }

    public int? ProjectId { get; set; }

    public DateOnly ReleaseDate { get; set; }

    public int Status { get; set; }

    public bool Deleted { get; set; }

    public virtual Project Project { get; set; }

    public virtual ICollection<ReleaseProject> ReleaseProjects { get; set; } = new List<ReleaseProject>();
}
