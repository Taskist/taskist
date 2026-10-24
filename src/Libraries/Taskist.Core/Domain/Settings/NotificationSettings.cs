using Taskist.Core.Domain.Common;

namespace Taskist.Core.Domain.Settings;

public class NotificationSettings : ISettings
{
    public bool EnableDailyDigest { get; set; } = true;

    public bool EnableOpenTaskDigest { get; set; } = true;

    public bool EnableProjectStatusDigest { get; set; } = true;

    public bool SummaryForProjectManager { get; set; } = true;

    public bool SummaryForTeamLead { get; set; } = true;
}
