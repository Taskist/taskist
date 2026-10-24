namespace Taskist.Core.Dtos.Git;

public class GitAccountStatusDto
{
    public int ConnectionId { get; set; }

    public string ConnectionName { get; set; } = string.Empty;

    public int ProviderType { get; set; }

    public string ProviderName { get; set; } = string.Empty;

    public string? WebUrl { get; set; }

    public bool Connected { get; set; }

    public bool OAuthAvailable { get; set; }

    public bool IsOAuth { get; set; }

    public string? AccountUserName { get; set; }

    public string? ExpectedUserName { get; set; }

    public string? Scopes { get; set; }

    public DateTime? ExpiresOn { get; set; }

    public DateTime? ConnectedOn { get; set; }

    public string? TokenHelpUrl { get; set; }

    public string? RequiredScopes { get; set; }

    public bool ExpiringSoon => !IsOAuth && ExpiresOn != null && ExpiresOn.Value.Date <= DateTime.UtcNow.Date.AddDays(7);

    public bool Expired => !IsOAuth && ExpiresOn != null && ExpiresOn.Value.Date < DateTime.UtcNow.Date;
}
