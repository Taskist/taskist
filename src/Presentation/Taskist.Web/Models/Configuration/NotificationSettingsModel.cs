namespace Taskist.Web.Models.Configuration;

public class NotificationSettingsModel
{
    public bool EnableDailyDigest { get; set; }

    public bool EnableOpenTaskDigest { get; set; }

    public bool EnableProjectStatusDigest { get; set; }

    public bool SummaryForProjectManager { get; set; }

    public bool SummaryForTeamLead { get; set; }
}
