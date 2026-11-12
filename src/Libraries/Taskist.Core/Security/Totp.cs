using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Taskist.Core.Security;

public static class Totp
{
    #region Fields

    public const int Digits = 6;

    public const int PeriodSeconds = 30;

    public const int SecretBytes = 20;

    public const int DefaultWindow = 1;

    #endregion

    #region Methods

    public static string GenerateSecret() => Base32.Encode(RandomNumberGenerator.GetBytes(SecretBytes));

    public static long TimeStep(DateTimeOffset time) => time.ToUnixTimeSeconds() / PeriodSeconds;

    public static string ComputeCode(byte[] key, long timeStep)
    {
        ArgumentNullException.ThrowIfNull(key);

        Span<byte> counter = stackalloc byte[8];
        BinaryPrimitives.WriteInt64BigEndian(counter, timeStep);

        var hash = HMACSHA1.HashData(key, counter);
        var offset = hash[^1] & 0x0F;
        var binary = ((hash[offset] & 0x7F) << 24)
            | (hash[offset + 1] << 16)
            | (hash[offset + 2] << 8)
            | hash[offset + 3];

        return (binary % 1_000_000).ToString("D6");
    }

    public static string? NormalizeCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        var digits = new string(code.Where(c => !char.IsWhiteSpace(c) && c != '-').ToArray());
        return digits.Length == Digits && digits.All(char.IsAsciiDigit) ? digits : null;
    }

    public static long? Verify(string? base32Secret, string? code, DateTimeOffset now, long lastUsedTimeStep, int window = DefaultWindow)
    {
        var key = Base32.Decode(base32Secret);
        var normalized = NormalizeCode(code);
        if (key == null || normalized == null)
            return null;

        var expected = Encoding.ASCII.GetBytes(normalized);
        var current = TimeStep(now);
        long? matched = null;

        for (var step = current - window; step <= current + window; step++)
        {
            var candidate = Encoding.ASCII.GetBytes(ComputeCode(key, step));
            if (CryptographicOperations.FixedTimeEquals(candidate, expected) && step > lastUsedTimeStep)
                matched ??= step;
        }

        return matched;
    }

    #endregion
}
