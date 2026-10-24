using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Taskist.Web.Models.Masters;

public class ProjectModel : BaseModel
{
    public ProjectModel()
    {
        AvailableClients = [new SelectListItem { Text = "Select", Value = "" }];
        AvailableProjectManagers = [new SelectListItem { Text = "None", Value = "" }];
        AvailableTeamLeads = [new SelectListItem { Text = "None", Value = "" }];
        AvailableCodeReviewers = [new SelectListItem { Text = "None", Value = "" }];
    }

    [LocalizedDisplayName("ProjectModel.Name")]
    public string Name { get; set; }

    [LocalizedDisplayName("ProjectModel.Description")]
    public string? Description { get; set; }

    [LocalizedDisplayName("ProjectModel.StartDate")]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
    public DateOnly? StartDate { get; set; }

    [LocalizedDisplayName("ProjectModel.EndDate")]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
    public DateOnly? EndDate { get; set; }

    [LocalizedDisplayName("ProjectModel.Client")]
    public int ClientId { get; set; }

    [LocalizedDisplayName("ProjectModel.ProjectManager")]
    public int? ProjectManagerId { get; set; }

    [LocalizedDisplayName("ProjectModel.TeamLead")]
    public int? TeamLeadId { get; set; }

    [LocalizedDisplayName("ProjectModel.CodeReviewer")]
    public int? CodeReviewerId { get; set; }

    [LocalizedDisplayName("ProjectModel.GitProjectReference")]
    public string? GitProjectReference { get; set; }

    [LocalizedDisplayName("ProjectModel.ReleaseVersionFormat")]
    public string? ReleaseVersionFormat { get; set; }

    [LocalizedDisplayName("ProjectModel.ReleaseFullIncrement")]
    public int? ReleaseFullIncrement { get; set; }

    public string? DefaultReleaseVersionFormat { get; set; }

    public bool GitLabConfigured { get; set; }

    public string? GitProjectName { get; set; }

    [LocalizedDisplayName("ProjectModel.Active")]
    public bool Active { get; set; } = true;

    public IList<SelectListItem> AvailableClients { get; set; }

    public IList<SelectListItem> AvailableProjectManagers { get; set; }

    public IList<SelectListItem> AvailableTeamLeads { get; set; }

    public IList<SelectListItem> AvailableCodeReviewers { get; set; }
}
