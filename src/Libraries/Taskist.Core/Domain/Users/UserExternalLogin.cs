using Taskist.Core.Domain.Common;

namespace Taskist.Core.Domain.Users;

public class UserExternalLogin : BaseEntity
{
    public int UserId { get; set; }

    public string Provider { get; set; }

    public string Subject { get; set; }

    public string? Email { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? LastUsedOn { get; set; }
}
