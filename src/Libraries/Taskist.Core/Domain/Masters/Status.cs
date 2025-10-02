using Taskist.Core.Domain.Common;

namespace Taskist.Core.Domain.Masters;

public class Status : BaseEntity, ISoftDeletedEntity
{
    public string Name { get; set; }

    public string? Description { get; set; }

    public int GroupId { get; set; }

    public string? TextColor { get; set; }

    public string? BackgroundColor { get; set; }

    public string? IconClass { get; set; }

    public bool SystemDefined { get; set; }

    public bool Active { get; set; }

    public bool Deleted { get; set; }
}