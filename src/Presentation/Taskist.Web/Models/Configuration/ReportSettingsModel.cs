namespace Taskist.Web.Models.Configuration;

public class ReportSettingsModel
{
    public string LayoutsJson { get; set; } = "{}";

    public List<ReportLayoutModel> Reports { get; set; } = [];
}
