using Taskist.Web.Helpers.Attributes;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Taskist.Web.Models.Configuration;

public class WorkItemSettingsModel
{
    public WorkItemSettingsModel()
    {
        AvailableUsers = [];
    }

    public int DefaultDueInDays { get; set; }

    public int MaxReopenCount { get; set; }

    public bool AllowSelfAssign { get; set; }

    public string? ReleaseVersionFormat { get; set; }

    public int ReleaseFullIncrement { get; set; }

    public IList<SelectListItem> AvailableUsers { get; set; }
}
