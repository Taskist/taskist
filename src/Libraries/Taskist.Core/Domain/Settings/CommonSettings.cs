using Taskist.Core.Domain.Common;

namespace Taskist.Core.Domain.Settings;

public class CommonSettings : ISettings
{
    public string AllowedIpAddress { get; set; }

    public string EncryptionKey { get; set; }

    public int SaltKeySize { get; set; }
}
