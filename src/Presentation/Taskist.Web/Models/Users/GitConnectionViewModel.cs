using Taskist.Core.Dtos.Git;

namespace Taskist.Web.Models.Users;

public class GitConnectionViewModel
{
    public GitAccountStatusDto Connection { get; set; } = new();
}
