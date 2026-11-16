using Taskist.Core.Enums;

namespace Taskist.Web.Helpers.Reports;

public interface IReportLayoutService
{
    List<ReportColumn> GetDefaultLayout(ReportExportKey key);

    Task<List<ReportColumn>> GetUserLayoutAsync(ReportExportKey key);

    Task<List<ReportColumn>> GetExportLayoutAsync(ReportExportKey key);

    Task<bool> HasCustomLayoutAsync(ReportExportKey key);

    Task SaveUserLayoutAsync(ReportExportKey key, IEnumerable<ReportColumn> columns);

    Task ResetUserLayoutAsync(ReportExportKey key);
}
