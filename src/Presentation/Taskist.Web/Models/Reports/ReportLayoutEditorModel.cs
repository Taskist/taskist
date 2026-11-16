using Taskist.Web.Models.Configuration;

namespace Taskist.Web.Models.Reports;

public class ReportLayoutEditorModel
{
    public bool IsCustom { get; set; }

    public ReportLayoutModel Layout { get; set; }
}
