using Taskist.Core.Domain.Masters;

namespace Taskist.Web.Models.WorkItems;

public class ReleasePlanIndexModel
{
	public IList<Project> Projects { get; set; } = [];

	public int MismatchCount { get; set; }
}
