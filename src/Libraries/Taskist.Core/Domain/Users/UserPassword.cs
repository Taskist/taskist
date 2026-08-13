using Taskist.Core.Domain.Common;

namespace Taskist.Core.Domain.Users;

public class UserPassword : BaseEntity
{
    public int UserId { get; set; }

    public string Password { get; set; }

    public string PasswordSalt { get; set; }

    /// <summary>
    /// Algorithm the stored hash was produced with. Legacy rows default to
    /// <see cref="PasswordFormat.Sha1Legacy"/> and are upgraded on next sign-in.
    /// </summary>
    public int HashFormat { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual User User { get; set; }
}