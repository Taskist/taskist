using Taskist.Core.Domain.Common;

namespace Taskist.Core.Domain.Users;

public class UserTwoFactor : BaseEntity
{
    public int UserId { get; set; }

    [Encrypted]
    public string Secret { get; set; }

    public DateTime EnabledOn { get; set; }

    public long LastUsedTimeStep { get; set; }
}
