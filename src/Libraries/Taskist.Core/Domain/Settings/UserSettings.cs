using Taskist.Core.Domain.Common;

namespace Taskist.Core.Domain.Settings;

public class UserSettings : ISettings
{
    public string Prefix { get; set; }

    public int Sequence { get; set; }
}
