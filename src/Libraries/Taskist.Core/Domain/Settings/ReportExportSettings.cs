using Taskist.Core.Domain.Common;

namespace Taskist.Core.Domain.Settings;

public class ReportExportSettings : ISettings
{
    public string LayoutsJson { get; set; } = "{}";
}
