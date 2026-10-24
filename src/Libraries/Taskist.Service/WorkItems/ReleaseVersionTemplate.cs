using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Taskist.Core.Enums;

namespace Taskist.Service.WorkItems;

public static class ReleaseVersionTemplate
{
    #region Constants

    public const string DefaultFormat = "{major}.{minor}.{patch}";

    public const int MaxLength = 50;

    private const string Major = "major";
    private const string Minor = "minor";
    private const string Patch = "patch";
    private const string Seq = "seq";

    private static readonly string[] Counters = [Major, Minor, Patch, Seq];

    private static readonly Dictionary<string, string> DateTokens = new()
    {
        ["yyyy"] = @"\d{4}",
        ["yy"] = @"\d{2}",
        ["MM"] = @"\d{2}",
        ["dd"] = @"\d{2}"
    };

    private static readonly Regex TokenPattern = new(@"\{([A-Za-z]+)\}", RegexOptions.Compiled);

    #endregion

    #region Methods

    public static bool IsValid(string? format)
    {
        return TryParse(format, out _);
    }

    public static string Next(string format, IEnumerable<string> existingNames, ReleaseType type,
        ReleaseVersionPart fullIncrement, DateOnly today, string? baseName = null)
    {
        if (!TryParse(format, out var tokens))
        {
            format = DefaultFormat;
            TryParse(format, out tokens);
        }

        var dates = DateValues(today);
        var regex = BuildRegex(tokens);
        var present = Counters.Where(c => tokens.Any(t => t.Name == c)).ToList();
        var level = BumpLevel(present, type, fullIncrement);

        var baseMatch = string.IsNullOrWhiteSpace(baseName) ? null : regex.Match(baseName.Trim());
        var fixedCounters = new Dictionary<string, long>();
        if (baseMatch is { Success: true })
        {
            dates = DateTokens.Keys.ToDictionary(d => d, d => baseMatch.Groups[d].Success ? baseMatch.Groups[d].Value : dates[d]);
            fixedCounters = present.Where(c => Array.IndexOf(Counters, c) < Array.IndexOf(Counters, level))
                .ToDictionary(c => c, c => ParseCounter(baseMatch, c));
            existingNames = existingNames.Append(baseName!);
        }

        var current = existingNames
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => regex.Match(n.Trim()))
            .Where(m => m.Success && DateTokens.Keys.All(d => !m.Groups[d].Success || m.Groups[d].Value == dates[d]))
            .Select(m => present.ToDictionary(c => c, c => ParseCounter(m, c)))
            .Where(v => fixedCounters.All(f => v[f.Key] == f.Value))
            .OrderByDescending(v => v.GetValueOrDefault(Major))
            .ThenByDescending(v => v.GetValueOrDefault(Minor))
            .ThenByDescending(v => v.GetValueOrDefault(Patch))
            .ThenByDescending(v => v.GetValueOrDefault(Seq))
            .FirstOrDefault();

        var values = current ?? present.ToDictionary(c => c, _ => 0L);
        if (current == null && type == ReleaseType.Full && present.Contains(Major))
            level = Major;

        values[level] = values[level] + 1;
        foreach (var lower in present.Where(c => Array.IndexOf(Counters, c) > Array.IndexOf(Counters, level)))
            values[lower] = 0;

        var builder = new StringBuilder();
        foreach (var token in tokens)
        {
            if (token.Name == null)
                builder.Append(token.Literal);
            else if (dates.TryGetValue(token.Name, out var date))
                builder.Append(date);
            else
                builder.Append(values[token.Name].ToString(CultureInfo.InvariantCulture));
        }

        return builder.ToString();
    }

    public static bool Matches(string format, string? name)
    {
        return !string.IsNullOrWhiteSpace(name) && TryParse(format, out var tokens) && BuildRegex(tokens).IsMatch(name.Trim());
    }

    #endregion

    #region Utilities

    private static bool TryParse(string? format, out List<(string? Name, string Literal)> tokens)
    {
        tokens = [];
        if (string.IsNullOrWhiteSpace(format) || format.Length > MaxLength)
            return false;

        var seen = new HashSet<string>();
        var position = 0;
        foreach (Match match in TokenPattern.Matches(format))
        {
            if (match.Index > position)
                tokens.Add((null, format[position..match.Index]));

            var name = match.Groups[1].Value;
            var key = Counters.Contains(name.ToLowerInvariant()) ? name.ToLowerInvariant() : name;
            if (!Counters.Contains(key) && !DateTokens.ContainsKey(key))
                return false;
            if (!seen.Add(key))
                return false;

            tokens.Add((key, match.Value));
            position = match.Index + match.Length;
        }

        if (position < format.Length)
            tokens.Add((null, format[position..]));

        if (tokens.Any(t => t.Name == null && (t.Literal.Contains('{') || t.Literal.Contains('}'))))
            return false;

        return tokens.Any(t => t.Name != null && Counters.Contains(t.Name));
    }

    private static Regex BuildRegex(List<(string? Name, string Literal)> tokens)
    {
        var pattern = new StringBuilder("^");
        foreach (var token in tokens)
        {
            if (token.Name == null)
                pattern.Append(Regex.Escape(token.Literal));
            else if (DateTokens.TryGetValue(token.Name, out var date))
                pattern.Append($"(?<{token.Name}>{date})");
            else
                pattern.Append($@"(?<{token.Name}>\d{{1,9}})");
        }
        pattern.Append('$');

        return new Regex(pattern.ToString(), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(200));
    }

    private static long ParseCounter(Match match, string counter)
    {
        return long.TryParse(match.Groups[counter].Value, out var value) ? value : 0;
    }

    private static string BumpLevel(List<string> present, ReleaseType type, ReleaseVersionPart fullIncrement)
    {
        var order = type == ReleaseType.Patch
            ? new[] { Patch, Seq, Minor, Major }
            : fullIncrement == ReleaseVersionPart.Major
                ? new[] { Major, Minor, Patch, Seq }
                : new[] { Minor, Major, Patch, Seq };

        return order.First(present.Contains);
    }

    private static Dictionary<string, string> DateValues(DateOnly today)
    {
        return new Dictionary<string, string>
        {
            ["yyyy"] = today.ToString("yyyy", CultureInfo.InvariantCulture),
            ["yy"] = today.ToString("yy", CultureInfo.InvariantCulture),
            ["MM"] = today.ToString("MM", CultureInfo.InvariantCulture),
            ["dd"] = today.ToString("dd", CultureInfo.InvariantCulture)
        };
    }

    #endregion
}
