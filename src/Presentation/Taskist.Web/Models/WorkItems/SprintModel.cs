using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.WorkItems;

public class SprintModel : BaseModel
{
    public SprintModel()
    {
        AvailableProjects = [];
    }

    [LocalizedDisplayName("SprintModel.Name")]
    public string Name { get; set; }

    [LocalizedDisplayName("SprintModel.Description")]
    public string? Description { get; set; }

    [LocalizedDisplayName("SprintModel.Project")]
    public int ProjectId { get; set; }

    [LocalizedDisplayName("SprintModel.StartDate")]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
    public DateOnly? StartDate { get; set; }

    [LocalizedDisplayName("SprintModel.EndDate")]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
    public DateOnly? EndDate { get; set; }

    public bool Started { get; set; }

    public IList<SelectListItem> AvailableProjects { get; set; }
}
