using Taskist.Core.Domain.Common;

namespace Taskist.Core.Domain.Masters;

public class Client : BaseEntity, ISoftDeletedEntity
{
    public string Name { get; set; }

    public string? Description { get; set; }

    public string? ContactPerson { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public string? Website { get; set; }

    public bool Active { get; set; }

    public bool Deleted { get; set; }
}