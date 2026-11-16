using System.Text;

namespace Taskist.Core.Security;

public static class Base32
{
    #region Fields

    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    #endregion

    #region Methods

    public static string Encode(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var output = new StringBuilder((data.Length * 8 + 4) / 5);
        int buffer = 0, bits = 0;

        foreach (var b in data)
        {
            buffer = (buffer << 8) | b;
            bits += 8;
            while (bits >= 5)
            {
                output.Append(Alphabet[(buffer >> (bits - 5)) & 31]);
                bits -= 5;
            }
        }

        if (bits > 0)
            output.Append(Alphabet[(buffer << (5 - bits)) & 31]);

        return output.ToString();
    }

    public static byte[]? Decode(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var output = new List<byte>(text.Length * 5 / 8);
        int buffer = 0, bits = 0;

        foreach (var raw in text)
        {
            if (raw == ' ' || raw == '-' || raw == '=')
                continue;

            var index = Alphabet.IndexOf(char.ToUpperInvariant(raw));
            if (index < 0)
                return null;

            buffer = (buffer << 5) | index;
            bits += 5;
            if (bits >= 8)
            {
                output.Add((byte)((buffer >> (bits - 8)) & 0xFF));
                bits -= 8;
            }
        }

        return output.Count == 0 ? null : output.ToArray();
    }

    public static string Group(string value, int size = 4) =>
        string.Join(' ', Enumerable.Range(0, (value.Length + size - 1) / size)
            .Select(i => value.Substring(i * size, Math.Min(size, value.Length - i * size))));

    #endregion
}
