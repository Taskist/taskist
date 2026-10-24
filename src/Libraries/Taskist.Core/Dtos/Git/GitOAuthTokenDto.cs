namespace Taskist.Core.Dtos.Git;

public class GitOAuthTokenDto
{
    public string AccessToken { get; set; } = string.Empty;

    public string? RefreshToken { get; set; }

    public DateTime? ExpiresOn { get; set; }

    public DateTime? RefreshExpiresOn { get; set; }

    public string? Scopes { get; set; }
}
