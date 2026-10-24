using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Taskist.Web.Models.WorkItems;

public class ReleaseModel : BaseModel
{
    public ReleaseModel()
    {
        AvailableProjects = [];
        SelectedProjectIds = [];
        AvailableBaseReleases = [];
        AllProjects = true;
    }

    [LocalizedDisplayName("ReleaseModel.Name")]
    public string Name { get; set; }

    [LocalizedDisplayName("ReleaseModel.ReleaseType")]
    public int ReleaseType { get; set; }

    [LocalizedDisplayName("ReleaseModel.Description")]
    public string? Description { get; set; }

    [LocalizedDisplayName("ReleaseModel.Project")]
    public int? ProjectId { get; set; }

    [LocalizedDisplayName("ReleaseModel.ProjectScope")]
    public bool AllProjects { get; set; }

    [LocalizedDisplayName("ReleaseModel.SelectedProjectIds")]
    public IList<int> SelectedProjectIds { get; set; }

    [LocalizedDisplayName("ReleaseModel.ReleaseDate")]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
    public DateOnly? ReleaseDate { get; set; }

    [LocalizedDisplayName("ReleaseModel.Status")]
    public int Status { get; set; }

    [LocalizedDisplayName("ReleaseModel.Deleted")]
    public bool Deleted { get; set; }

    public bool VersionAuto { get; set; }

    [LocalizedDisplayName("ReleaseModel.BaseReleaseId")]
    public int? BaseReleaseId { get; set; }

    public string? VersionFormat { get; set; }

    public bool ConfirmOpenTasks { get; set; }

    public int? CarryOverReleaseId { get; set; }

    public IList<SelectListItem> AvailableProjects { get; set; }

    public IList<SelectListItem> AvailableBaseReleases { get; set; }
}
