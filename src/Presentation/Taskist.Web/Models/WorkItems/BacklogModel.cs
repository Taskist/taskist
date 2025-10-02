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
        AvailableTaskTypes = [];
        AvailableReporters = [new SelectListItem { Text = "None", Value = "-1" }];
        AvailableSeverities = [new SelectListItem { Text = "Select", Value = "" }];
        AvailableModules = [new SelectListItem { Text = "Select", Value = "" }];
        AvailableSubModules = [new SelectListItem { Text = "Select", Value = "" }];
        AvailableSprints = [new SelectListItem { Text = "No Sprint", Value = "-1" }];
        AvailableAssignees = [new SelectListItem { Text = "Unassigned", Value = "-1" }];
        AvailableStatus = [];
        CustomFieldValue = new CustomFieldValueModel();
    }

    public Guid Code { get; set; }

    [LocalizedDisplayName("BacklogTask.Title")]
    public string Title { get; set; }

    [LocalizedDisplayName("BacklogTask.Description")]
    public string? Description { get; set; }

    [LocalizedDisplayName("BacklogTask.DeveloperNotes")]
    public string? DeveloperNotes { get; set; }

    [LocalizedDisplayName("BacklogTask.QualityNotes")]
    public string? QualityNotes { get; set; }

    [LocalizedDisplayName("BacklogTask.Parent")]
    public int? ParentId { get; set; }

    [LocalizedDisplayName("BacklogTask.TaskType")]
    public int TaskTypeId { get; set; }

    [LocalizedDisplayName("BacklogTask.Reporter")]
    public int? ReporterId { get; set; }

    [LocalizedDisplayName("BacklogTask.Severity")]
    public int SeverityId { get; set; }

    [LocalizedDisplayName("BacklogTask.DueDate")]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
    public DateOnly? DueDate { get; set; }

    [LocalizedDisplayName("BacklogTask.Project")]
    public int ProjectId { get; set; }

    [LocalizedDisplayName("BacklogTask.Module")]
    public int? ModuleId { get; set; }

    [LocalizedDisplayName("BacklogTask.SubModule")]
    public int? SubModuleId { get; set; }

    [LocalizedDisplayName("BacklogTask.Sprint")]
    public int? SprintId { get; set; }

    [LocalizedDisplayName("BacklogTask.Assignee")]
    public int? AssigneeId { get; set; } = null;

    [LocalizedDisplayName("BacklogTask.Status")]
    public int StatusId { get; set; }

    public int ReOpenCount { get; set; }

    public int SubTaskCount { get; set; }

    public string Token { get; set; }

    public bool CanEdit { get; set; }

    [LocalizedDisplayName("BacklogTask.CreatedBy")]
    public int CreatedById { get; set; }

    public CustomFieldValueModel CustomFieldValue { get; set; }

    public IList<SelectListItem> AvailableTaskTypes { get; set; }

    public IList<SelectListItem> AvailableReporters { get; set; }

    public IList<SelectListItem> AvailableSeverities { get; set; }

    public IList<SelectListItem> AvailableModules { get; set; }

    public IList<SelectListItem> AvailableSubModules { get; set; }

    public IList<SelectListItem> AvailableSprints { get; set; }

    public IList<SelectListItem> AvailableAssignees { get; set; }

    public IList<SelectListItem> AvailableStatus { get; set; }
}

public class BacklogItemGridModel : BaseModel
{
    public string Title { get; set; }

    public DateOnly? DueDate { get; set; }

    public string Project { get; set; }

    public string? Module { get; set; }

    public string? SubModule { get; set; }

    public string? Sprint { get; set; }

    public string? Assignee { get; set; }

    public int SubTaskCount { get; set; }

    public string CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    public StatusModel Status { get; set; }

    public SeverityModel Severity { get; set; }

    public TaskTypeModel TaskType { get; set; }
}
