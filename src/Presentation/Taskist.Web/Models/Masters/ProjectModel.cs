using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.Masters;

public class ProjectModel : BaseModel
{
    public ProjectModel()
    {
        AvailableClients = [new SelectListItem { Text = "Select", Value = "" }];
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

    [LocalizedDisplayName("ProjectModel.Active")]
    public bool Active { get; set; } = true;

    public IList<SelectListItem> AvailableClients { get; set; }
}

public class ProjectGridModel : BaseModel
{
    public string Name { get; set; }

    public string? Description { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string ClientName { get; set; }

    public bool Active { get; set; }
}

public class ProjectSelectorModel : BaseModel
{
    public string Name { get; set; }

    public bool Selected { get; set; }
}
