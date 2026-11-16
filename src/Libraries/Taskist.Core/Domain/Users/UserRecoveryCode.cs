using Taskist.Core.Domain.Common;

namespace Taskist.Core.Domain.Users;

public class UserRecoveryCode : BaseEntity
{
    public int UserId { get; set; }

    public string CodeHash { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? UsedOn { get; set; }
}
