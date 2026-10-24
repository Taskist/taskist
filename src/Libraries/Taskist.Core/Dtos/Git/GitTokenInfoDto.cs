namespace Taskist.Core.Dtos.Git;

public class GitTokenInfoDto
{
    public string ExternalUserId { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string? DisplayName { get; set; }

    public List<string> Scopes { get; set; } = [];

    public DateTime? ExpiresAt { get; set; }

    public string? Problem { get; set; }
}
