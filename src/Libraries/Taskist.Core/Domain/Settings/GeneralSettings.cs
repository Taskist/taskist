using Taskist.Core.Domain.Common;

namespace Taskist.Core.Domain.Settings;

public class GeneralSettings : ISettings
{
    public string? CompanyName { get; set; }

    public string? SupportEmail { get; set; }

    public int ItemsPerPage { get; set; } = 15;
}
