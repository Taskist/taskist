namespace Taskist.Web.Models.Users;

public class GitOAuthSessionModel
{
    public string State { get; set; } = string.Empty;

    public string Verifier { get; set; } = string.Empty;

    public int ConnectionId { get; set; }

    public int UserId { get; set; }

    public DateTime CreatedOn { get; set; }
}
