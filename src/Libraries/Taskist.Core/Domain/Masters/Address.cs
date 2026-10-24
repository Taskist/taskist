using Taskist.Core.Domain.Common;

namespace Taskist.Core.Domain.Masters;

public class Address : BaseEntity, IAuditableEntity
{
    public string Address1 { get; set; }

    public string? Address2 { get; set; }

    public string? City { get; set; }

    public string ZipPostalCode { get; set; }
}
