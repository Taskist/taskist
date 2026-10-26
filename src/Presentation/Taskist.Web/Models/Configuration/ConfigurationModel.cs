namespace Taskist.Web.Models.Configuration;

public class ConfigurationModel
{
    public string ActiveTab { get; set; } = "general";

    public bool CanGeneral { get; set; }

    public bool CanSecurity { get; set; }

    public bool CanWorkItem { get; set; }

    public bool CanIntegration { get; set; }

    public bool CanReports { get; set; }

    public bool CanResource { get; set; }

    public bool CanStageDuration { get; set; }

    public GeneralSettingsModel General { get; set; } = new();

    public SecuritySettingsModel Security { get; set; } = new();

    public WorkItemSettingsModel WorkItem { get; set; } = new();

    public IntegrationSettingsModel Integration { get; set; } = new();

    public ReportSettingsModel Reports { get; set; } = new();

    public ResourceSettingsModel Resource { get; set; } = new();

    public StageDurationSettingsModel StageDuration { get; set; } = new();
}
