using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Taskist.Core.Security;

namespace Taskist.Data.Configuration;

public class EncryptedStringConverter : ValueConverter<string, string>
{
    public EncryptedStringConverter(ISecretProtector? protector)
        : base(v => Encode(protector, v), v => Decode(protector, v))
    {
    }

    private static string Encode(ISecretProtector? protector, string value)
        => protector == null || string.IsNullOrEmpty(value) ? value : protector.Protect(value)!;

    private static string Decode(ISecretProtector? protector, string value)
        => protector == null || string.IsNullOrEmpty(value) ? value : protector.Unprotect(value) ?? string.Empty;
}
