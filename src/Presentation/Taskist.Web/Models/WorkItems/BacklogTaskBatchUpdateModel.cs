using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Taskist.Web.Models.WorkItems;

public class BacklogBatchUpdateModel : BaseModel
{
	public BacklogBatchUpdateModel()
	{
		AvailableTaskTypes = [new SelectListItem { Text = "Select", Value = "-1" }];
		AvailableReporters = [new SelectListItem { Text = "Select", Value = "-1" }];
		AvailableSeverities = [new SelectListItem { Text = "Select", Value = "" }];
		AvailableModules = [new SelectListItem { Text = "Select", Value = "" }];
		AvailableSubModules = [new SelectListItem { Text = "Select", Value = "" }];
		AvailableReleases = [new SelectListItem { Text = "Select", Value = "-1" }];
		AvailableAssignees = [new SelectListItem { Text = "Select", Value = "-1" }];
		AvailableTesters = [new SelectListItem { Text = "Select", Value = "-1" }];
		AvailableStatus = [new SelectListItem { Text = "Select", Value = "-1" }];
	}

	public string TaskIds { get; set; }

	public bool IsMixedProject { get; set; }

	[LocalizedDisplayName("Backlog.TaskType")]
	public int? TaskTypeId { get; set; }

	[LocalizedDisplayName("Backlog.Reporter")]
	public int? ReporterId { get; set; }

	[LocalizedDisplayName("Backlog.Severity")]
	public int? SeverityId { get; set; }

	[LocalizedDisplayName("Backlog.DueDate")]
	[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
	public DateOnly? DueDate { get; set; }

	[LocalizedDisplayName("Backlog.Module")]
	public int? ModuleId { get; set; }

	[LocalizedDisplayName("Backlog.SubModule")]
	public int? SubModuleId { get; set; }

	[LocalizedDisplayName("Backlog.Release")]
	public int? ReleaseId { get; set; }

	[LocalizedDisplayName("Backlog.Assignee")]
	public int? AssigneeId { get; set; } = null;

	[LocalizedDisplayName("Backlog.Tester")]
	public int? TesterId { get; set; } = null;

	[LocalizedDisplayName("Backlog.Status")]
	public int? StatusId { get; set; }

	public IList<SelectListItem> AvailableTaskTypes { get; set; }

	public IList<SelectListItem> AvailableReporters { get; set; }

	public IList<SelectListItem> AvailableSeverities { get; set; }

	public IList<SelectListItem> AvailableModules { get; set; }

	public IList<SelectListItem> AvailableSubModules { get; set; }

	public IList<SelectListItem> AvailableReleases { get; set; }

	public IList<SelectListItem> AvailableAssignees { get; set; }

	public IList<SelectListItem> AvailableTesters { get; set; }

	public IList<SelectListItem> AvailableStatus { get; set; }
}
