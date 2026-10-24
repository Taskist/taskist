using Taskist.Core.Domain.Common;

namespace Taskist.Core.Domain.Users;

public class UserRoleMap : BaseEntity, IAuditableEntity
{
    public int UserId { get; set; }

    public int UserRoleId { get; set; }

    public virtual User User { get; set; }

    public virtual UserRole UserRole { get; set; }
}