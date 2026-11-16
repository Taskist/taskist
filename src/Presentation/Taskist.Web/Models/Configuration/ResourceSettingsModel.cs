using Taskist.Web.Helpers.Attributes;

namespace Taskist.Web.Models.Configuration;

public class ResourceSettingsModel
{
    [LocalizedDisplayName("ResourceSettingsModel.LightMax")]
    public int LightMax { get; set; }

    [LocalizedDisplayName("ResourceSettingsModel.BalancedMax")]
    public int BalancedMax { get; set; }

    [LocalizedDisplayName("ResourceSettingsModel.DueSoonDays")]
    public int DueSoonDays { get; set; }

    [LocalizedDisplayName("ResourceSettingsModel.CriticalWeight")]
    public double CriticalWeight { get; set; }

    [LocalizedDisplayName("ResourceSettingsModel.HighWeight")]
    public double HighWeight { get; set; }

    [LocalizedDisplayName("ResourceSettingsModel.MediumWeight")]
    public double MediumWeight { get; set; }

    [LocalizedDisplayName("ResourceSettingsModel.LowWeight")]
    public double LowWeight { get; set; }
}
