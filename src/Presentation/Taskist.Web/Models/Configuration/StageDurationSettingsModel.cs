using Microsoft.AspNetCore.Mvc.Rendering;

namespace Taskist.Web.Models.Configuration;

public class StageDurationSettingsModel
{
    public string MetricsJson { get; set; } = "[]";

    public List<StageDurationRowModel> Metrics { get; set; } = [];

    public List<SelectListItem> AvailableStatuses { get; set; } = [];

    public List<SelectListItem> AvailableProjects { get; set; } = [];
}
