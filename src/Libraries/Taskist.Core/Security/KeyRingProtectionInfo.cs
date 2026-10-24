namespace Taskist.Core.Security;

public class KeyRingProtectionInfo
{
    public bool ProtectedAtRest { get; init; }

    public string Method { get; init; } = "none";

    public string? KeysPath { get; init; }
}
