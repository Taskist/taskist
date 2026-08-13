namespace Taskist.Web.Models.Dashboard;

public class DashboardModel
{
    // filter state + options
    public int SelectedProjectId { get; set; }
    public int Days { get; set; }
    public List<DashboardProjectOption> ProjectOptions { get; set; } = new();

    // KPI row
    public int TotalTasks { get; set; }
    public int MyTasks { get; set; }
    public int OpenTasks { get; set; }
    public int ClosedTasks { get; set; }
    public int OverdueTasks { get; set; }
    public int DueSoonTasks { get; set; }

    // charts (label -> count, with colours)
    public List<DashboardSlice> StatusDistribution { get; set; } = new();
    public List<DashboardSlice> SeverityDistribution { get; set; } = new();
    public List<DashboardSlice> TaskTypeDistribution { get; set; } = new();

    // due-date breakdown (open tasks only) - Today / Tomorrow / Next 7 days / Overdue / No due date
    public int DueToday { get; set; }
    public int DueTomorrow { get; set; }
    public int DueNext7 { get; set; }
    public int DueOverdue { get; set; }
    public int DueNone { get; set; }

    // actionable lists
    public List<DashboardTask> MyOpenTasks { get; set; } = new();
    public List<DashboardActivity> RecentActivity { get; set; } = new();
}

public class DashboardProjectOption
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class DashboardSlice
{
    public string Label { get; set; }
    public int Count { get; set; }
    public string Color { get; set; }
}

public class DashboardTask
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string StatusName { get; set; }
    public string StatusColor { get; set; }
    public string SeverityName { get; set; }
    public string SeverityColor { get; set; }
    public DateOnly? DueDate { get; set; }
    public bool IsOverdue { get; set; }
}

public class DashboardActivity
{
    public int BacklogId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string Text { get; set; }
    public DateTime When { get; set; }
    public bool IsSystem { get; set; }
}
