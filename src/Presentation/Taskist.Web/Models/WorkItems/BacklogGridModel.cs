namespace Taskist.Web.Models.WorkItems;

public class BacklogGridModel
{
	public string UserGuid { get; set; }

	public int ProjectId { get; set; }

	public bool CanReport { get; set; }

	public bool CanReportToDo { get; set; }

	public bool CanReportCR { get; set; }

	public bool CanReportBug { get; set; }

	public bool CanEdit { get; set; }

	public bool CanClose { get; set; }

	public bool CanReOpen { get; set; }

	public bool CanComment { get; set; }

	public bool CanViewOthersTask { get; set; }

	public bool CanEditOthersTask { get; set; }

	public bool CanCreateBranch { get; set; }

	public int FilterMode { get; set; }

	public string? FilterValue { get; set; }

	public string? FilterLabel { get; set; }

	public int FilterProjectId { get; set; }

	public int FilterAssigneeId { get; set; }
}
