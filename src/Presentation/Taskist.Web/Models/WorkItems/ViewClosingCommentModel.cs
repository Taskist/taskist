namespace Taskist.Web.Models.WorkItems;

public class ViewClosingCommentModel
{
	public int TaskId { get; set; }

	public int ProjectId { get; set; }

	public string? RootCauseAnalysis { get; set; }

	public string? ResolutionDetails { get; set; }

	public int? MergeRequestId { get; set; }

	public string RefPrefix { get; set; } = "!";
}
