using Taskist.Core.Domain.Common;
using Taskist.Core.Domain.Users;

namespace Taskist.Core.Domain.Git;

public class UserGitAccount : BaseEntity, IAuditableEntity
{
    public int UserId { get; set; }

    public int GitConnectionId { get; set; }

    public string ExternalUserId { get; set; }

    public string UserName { get; set; }

    public string? DisplayName { get; set; }

    public int TokenType { get; set; }

    [NotAudited]
    [Encrypted]
    public string AccessToken { get; set; }

    [NotAudited]
    [Encrypted]
    public string? RefreshToken { get; set; }

    public string? Scopes { get; set; }

    public DateTime? ExpiresOn { get; set; }

    public DateTime? RefreshExpiresOn { get; set; }

    public DateTime ConnectedOn { get; set; }

    public DateTime? LastUsedOn { get; set; }

    public virtual User User { get; set; }

    public virtual GitConnection GitConnection { get; set; }
}
