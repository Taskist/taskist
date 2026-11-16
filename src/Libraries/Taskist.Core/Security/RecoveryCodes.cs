using System.Security.Cryptography;
using System.Text;

namespace Taskist.Core.Security;

public static class RecoveryCodes
{
    #region Fields

    public const int DefaultCount = 10;

    private const string Alphabet = "23456789abcdefghjkmnpqrstuvwxyz";

    private const int GroupLength = 5;

    #endregion

    #region Methods

    public static IReadOnlyList<string> Generate(int count = DefaultCount)
    {
        var codes = new HashSet<string>(StringComparer.Ordinal);
        while (codes.Count < count)
        {
            var chars = new char[GroupLength * 2];
            for (var i = 0; i < chars.Length; i++)
                chars[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];

            codes.Add($"{new string(chars, 0, GroupLength)}-{new string(chars, GroupLength, GroupLength)}");
        }

        return codes.ToList();
    }

    public static string? Normalize(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        var normalized = new string(code.Where(c => !char.IsWhiteSpace(c) && c != '-').ToArray()).ToLowerInvariant();
        return normalized.Length == GroupLength * 2 && normalized.All(c => Alphabet.Contains(c)) ? normalized : null;
    }

    public static string? Hash(string? code)
    {
        var normalized = Normalize(code);
        if (normalized == null)
            return null;

        return Convert.ToHexStringLower(SHA256.HashData(Encoding.ASCII.GetBytes(normalized)));
    }

    #endregion
}
