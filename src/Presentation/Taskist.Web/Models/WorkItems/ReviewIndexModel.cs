using Taskist.Core.Dtos.Git;

namespace Taskist.Web.Models.WorkItems;

public class ReviewIndexModel
{
	public bool GitConfigured { get; set; }

	public int ProjectCount { get; set; }

	public IList<GitAccountStatusDto> Connections { get; set; } = [];

	public bool AnyDisconnected => Connections.Any(c => !c.Connected || c.Expired);

	public bool CanCleanupBranches { get; set; }

	public int UnlinkedBranchCount { get; set; }
}
