using Taskist.Core.Domain.Common;

namespace Taskist.Core.Domain.Git;

public class GitConnection : BaseEntity, ISoftDeletedEntity, IAuditableEntity
{
    public string Name { get; set; }

    public int ProviderType { get; set; }

    public string BaseUrl { get; set; }

    public string? ApiUrl { get; set; }

    public int AuthMode { get; set; }

    [NotAudited]
    [Encrypted]
    public string? ServiceToken { get; set; }

    public string? AppId { get; set; }

    [NotAudited]
    [Encrypted]
    public string? AppPrivateKey { get; set; }

    public string? OAuthClientId { get; set; }

    [NotAudited]
    [Encrypted]
    public string? OAuthClientSecret { get; set; }

    public string? ServiceAccountUserName { get; set; }

    public string? ServiceAccountExternalId { get; set; }

    public bool AllowInsecureHttp { get; set; }

    public bool IsDefault { get; set; }

    public bool Active { get; set; }

    public bool Deleted { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }
}
