using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;
using Taskist.Web.Models.Masters;

namespace Taskist.Web.Models.WorkItems;

public class BacklogModel : BaseModel
{
	public BacklogModel()
	{
		StatusLogs = [];
		AvailableProjects = [new SelectListItem { Text = "None", Value = "-1" }];
		AvailableTaskTypes = [];
		AvailableReporters = [new SelectListItem { Text = "Select", Value = "" }];
		AvailableSeverities = [new SelectListItem { Text = "Select", Value = "" }];
		AvailableModules = [new SelectListItem { Text = "Select", Value = "" }];
		AvailableSubModules = [new SelectListItem { Text = "Select", Value = "" }];
		AvailableReleases = [new SelectListItem { Text = "No Release", Value = "-1" }];
		AvailableAssignees = [new SelectListItem { Text = "Unassigned", Value = "-1" }];
		AvailableCreators = [new SelectListItem { Text = "Unassigned", Value = "-1" }];
		AvailableTesters = [new SelectListItem { Text = "Unassigned", Value = "-1" }];
		AvailableStatus = [];
		AvailableStatusGroup = [];
		AvailableTags = [];
		SelectedTags = [];
		CustomFieldValue = new CustomFieldValueModel();
	}

	public Guid Code { get; set; }

	[LocalizedDisplayName("Backlog.Title")]
	public string Title { get; set; }

	[LocalizedDisplayName("Backlog.Description")]
	public string? Description { get; set; }

	[LocalizedDisplayName("Backlog.RootCauseAnalysis")]
	public string? RootCauseAnalysis { get; set; }

	public bool IsBug { get; set; }

	public string? BugTaskTypeIds { get; set; }

	public string? CrTaskTypeIds { get; set; }

	public bool ShowRootCauseTab { get; set; }

	public string? ClosingRulesJson { get; set; }

	public string? EnvironmentJson { get; set; }

	[LocalizedDisplayName("Backlog.ExternalNotes")]
	public string? ExternalNotes { get; set; }

	[LocalizedDisplayName("Backlog.Parent")]
	public int? ParentId { get; set; }

	public string? ParentTitle { get; set; }

	[LocalizedDisplayName("Backlog.TaskType")]
	public int TaskTypeId { get; set; }

	[LocalizedDisplayName("Backlog.Reporter")]
	public int? ReporterId { get; set; }

	[LocalizedDisplayName("Backlog.Severity")]
	public int SeverityId { get; set; }

	[LocalizedDisplayName("Backlog.DueDate")]
	[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
	public DateOnly? DueDate { get; set; }

	[LocalizedDisplayName("Backlog.PlannedStartDate")]
	[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
	public DateOnly? PlannedStartDate { get; set; }

	[LocalizedDisplayName("Backlog.Project")]
	public int ProjectId { get; set; }

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
	public int StatusId { get; set; }

	[LocalizedDisplayName("Backlog.StatusGroup")]
	public int StatusGroupId { get; set; }

	[LocalizedDisplayName("ClosingCommentModel.ResolutionDetails")]
	public string? ResolutionDetails { get; set; }

	public int? MergeRequestId { get; set; }

	[LocalizedDisplayName("Backlog.Tags")]
	public string[] SelectedTags { get; set; }

	public int? SourceWikiDocumentId { get; set; }

	public int ReOpenCount { get; set; }

	public int SubTaskCount { get; set; }

	public string Token { get; set; }

	public bool CanEdit { get; set; }

	public bool CanEditProject { get; set; }

	public bool CanDelete { get; set; }

	[LocalizedDisplayName("Backlog.CreatedBy")]
	public int CreatedById { get; set; }

	public string Owner { get; set; }

	public IList<StatusLogModel> StatusLogs { get; set; }

	public IList<StageDurationChipModel> StageDurations { get; set; } = [];

	public CustomFieldValueModel CustomFieldValue { get; set; }

	public IList<SelectListItem> AvailableProjects { get; set; }

	public IList<SelectListItem> AvailableTaskTypes { get; set; }

	public IList<SelectListItem> AvailableReporters { get; set; }

	public IList<SelectListItem> AvailableSeverities { get; set; }

	public IList<SelectListItem> AvailableModules { get; set; }

	public IList<SelectListItem> AvailableSubModules { get; set; }

	public IList<SelectListItem> AvailableReleases { get; set; }

	public IList<SelectListItem> AvailableAssignees { get; set; }

	public IList<SelectListItem> AvailableCreators { get; set; }

	public IList<SelectListItem> AvailableTesters { get; set; }

	public IList<SelectListItem> AvailableStatus { get; set; }

	public IList<SelectListItem> AvailableStatusGroup { get; set; }

	public IList<SelectListItem> AvailableTags { get; set; }
}
