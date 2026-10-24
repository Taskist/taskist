namespace Taskist.Web.Models.WorkItems;

public class StageDurationChipModel
{
	public string Name { get; set; }

	public string FromStatus { get; set; }

	public string ToStatus { get; set; }

	public string DurationText { get; set; }

	public DateTime From { get; set; }

	public DateTime To { get; set; }
}
