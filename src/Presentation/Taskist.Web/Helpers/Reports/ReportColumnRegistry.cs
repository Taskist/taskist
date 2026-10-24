using Taskist.Core.Enums;

namespace Taskist.Web.Helpers.Reports;

public static class ReportColumnRegistry
{
    private static readonly Dictionary<ReportExportKey, List<ReportColumn>> _catalog = new()
    {
        [ReportExportKey.Backlogs] =
        [
            new() { Key = "Id", Title = "ID" },
            new() { Key = "Title", Title = "Title" },
            new() { Key = "Description", Title = "Description" },
            new() { Key = "RootCauseAnalysis", Title = "Root Cause Analysis" },
            new() { Key = "ResolutionDetails", Title = "Resolution" },
            new() { Key = "TaskType", Title = "Task Type" },
            new() { Key = "Severity", Title = "Severity" },
            new() { Key = "Status", Title = "Status" },
            new() { Key = "Project", Title = "Project" },
            new() { Key = "Module", Title = "Module" },
            new() { Key = "SubModule", Title = "Sub-Module" },
            new() { Key = "Release", Title = "Release" },
            new() { Key = "Assignee", Title = "Assignee" },
            new() { Key = "Tester", Title = "Tester" },
            new() { Key = "DueDate", Title = "Due Date" },
            new() { Key = "ReOpenCount", Title = "Re-open Count" },
            new() { Key = "SubTaskCount", Title = "Sub-task Count" },
            new() { Key = "CreatedBy", Title = "Created By" },
            new() { Key = "CreatedOn", Title = "Created On" },
            new() { Key = "AgeDays", Title = "Age (days)" },
            new() { Key = "Tags", Title = "Tags" },
        ],
        [ReportExportKey.ReleaseTasks] =
        [
            new() { Key = "Id", Title = "ID" },
            new() { Key = "Project", Title = "Product" },
            new() { Key = "Module", Title = "Module" },
            new() { Key = "SubModule", Title = "Sub-Module" },
            new() { Key = "Title", Title = "Title" },
            new() { Key = "Description", Title = "Description" },
            new() { Key = "RootCauseAnalysis", Title = "Root Cause Analysis" },
            new() { Key = "ResolutionDetails", Title = "Resolution" },
            new() { Key = "Release", Title = "Release" },
            new() { Key = "TaskType", Title = "Task Type" },
            new() { Key = "Severity", Title = "Severity" },
            new() { Key = "Status", Title = "Status" },
            new() { Key = "Assignee", Title = "Developer" },
            new() { Key = "Tester", Title = "Tester" },
            new() { Key = "DueDate", Title = "Due Date" },
            new() { Key = "CreatedBy", Title = "Created By" },
            new() { Key = "CreatedOn", Title = "Created On" },
            new() { Key = "Tags", Title = "Tags" },
        ],
    };

    public static IReadOnlyList<ReportColumn> GetColumns(ReportExportKey key)
    {
        return _catalog.TryGetValue(key, out var columns) ? columns : [];
    }

    public static string GetName(ReportExportKey key) => key switch
    {
        ReportExportKey.Backlogs => "Backlog Export",
        ReportExportKey.ReleaseTasks => "Release Export",
        _ => key.ToString()
    };

    public static IReadOnlyDictionary<ReportExportKey, List<ReportColumn>> All => _catalog;
}
