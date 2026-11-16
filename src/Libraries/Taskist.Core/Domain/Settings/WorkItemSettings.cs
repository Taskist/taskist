using Taskist.Core.Domain.Common;

namespace Taskist.Core.Domain.Settings;

public class WorkItemSettings : ISettings
{
    public int DefaultDueInDays { get; set; } = 7;

    public int MaxReopenCount { get; set; } = 0;

    public bool AllowSelfAssign { get; set; } = true;

    public string ReleaseVersionFormat { get; set; } = "{major}.{minor}.{patch}";

    public int ReleaseFullIncrement { get; set; } = 2;
}
