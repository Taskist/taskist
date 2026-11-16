using Taskist.Web.Models.Masters;

namespace Taskist.Web.Models.WorkItems;

public class StatusLogModel
{
	public StatusModel Status { get; set; } = new();

	public int DaysHeld { get; set; }

	public DateTime? StartDate { get; set; }

	public DateTime? EndDate { get; set; }
}