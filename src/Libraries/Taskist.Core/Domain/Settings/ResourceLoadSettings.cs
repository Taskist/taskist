using Taskist.Core.Domain.Common;

namespace Taskist.Core.Domain.Settings;

public class ResourceLoadSettings : ISettings
{
    public int LightMax { get; set; } = 3;

    public int BalancedMax { get; set; } = 8;

    public int DueSoonDays { get; set; } = 7;

    public double CriticalWeight { get; set; } = 3;

    public double HighWeight { get; set; } = 2;

    public double MediumWeight { get; set; } = 1.5;

    public double LowWeight { get; set; } = 1;
}
