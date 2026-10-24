using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Taskist.Core.Enums;

namespace Taskist.Core.Common;

public static class BranchNameBuilder
{
    #region Fields

    private const int MaxIdsInName = 3;
    private const int MaxSlugLength = 40;
    private const int MaxNameLength = 100;

    public static readonly IReadOnlyList<string> Prefixes = ["feature", "bugfix", "task", "hotfix"];

    #endregion

    #region Methods

    public static string PrefixFor(IEnumerable<int> taskTypeGroups)
    {
        var groups = taskTypeGroups.Distinct().ToList();
        if (groups.Count == 1 && groups[0] == (int)TaskTypeGroup.Bug)
            return "bugfix";
        if (groups.Count == 1 && groups[0] == (int)TaskTypeGroup.ToDo)
            return "task";
        return "feature";
    }

    public static string Build(string prefix, IList<int> taskIds, string? firstTitle, string? keyPrefix)
    {
        var ids = taskIds.Distinct().OrderBy(x => x).ToList();
        var key = string.IsNullOrWhiteSpace(keyPrefix) ? string.Empty : keyPrefix.Trim().ToUpperInvariant() + "-";

        var idPart = ids.Count <= MaxIdsInName
            ? string.Join("-", ids.Select(x => key + x))
            : $"{key}{ids[0]}-plus-{ids.Count - 1}";

        var slug = Slugify(firstTitle, MaxSlugLength);
        var name = $"{prefix}/{idPart}" + (slug.Length > 0 ? "-" + slug : string.Empty);

        return name.Length <= MaxNameLength ? name : name[..MaxNameLength].TrimEnd('-');
    }

    public static string Slugify(string? text, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                continue;

            if (char.IsLetterOrDigit(c) && c < 128)
                sb.Append(char.ToLowerInvariant(c));
            else if (sb.Length > 0 && sb[^1] != '-')
                sb.Append('-');
        }

        var slug = sb.ToString().Trim('-');
        if (slug.Length > maxLength)
            slug = slug[..maxLength].TrimEnd('-');
        return slug;
    }

    public static IList<int> ExtractTaskIds(string? branchName, string? keyPrefix)
    {
        var result = new List<int>();
        if (string.IsNullOrWhiteSpace(branchName))
            return result;

        var tail = branchName.Contains('/') ? branchName[(branchName.LastIndexOf('/') + 1)..] : branchName;
        var key = string.IsNullOrWhiteSpace(keyPrefix) ? null : keyPrefix.Trim().ToUpperInvariant();
        var tokens = tail.Split('-', StringSplitOptions.RemoveEmptyEntries);

        for (var i = 0; i < tokens.Length; i++)
        {
            var token = tokens[i];
            if (key != null && string.Equals(token, key, StringComparison.OrdinalIgnoreCase) && i + 1 < tokens.Length)
                token = tokens[++i];

            if (!int.TryParse(token, out var id) || id <= 0)
                break;

            result.Add(id);
        }

        return result;
    }

    public static ISet<int> ExtractTaskReferences(string? keyPrefix, params string?[] texts)
    {
        var result = new HashSet<int>();
        foreach (var text in texts)
        {
            if (string.IsNullOrWhiteSpace(text))
                continue;

            var withoutUrls = Regex.Replace(text, @"\b(?:https?|ftp)://\S+", " ", RegexOptions.IgnoreCase);

            foreach (var id in ExtractMentionedTaskIds(withoutUrls, keyPrefix))
                result.Add(id);

            foreach (Match m in Regex.Matches(withoutUrls, @"(?<![\w.\-:#!])(\d{3,9})(?![\w]|[.:/\-]\d)"))
                result.Add(int.Parse(m.Groups[1].Value));
        }

        return result;
    }

    public static IList<int> TasksForBranchMergeRequest(IList<int> linkedTaskIds, ISet<int> knownTaskIds,
        string? sourceBranch, string? keyPrefix, string? title, string? description,
        bool isGroupBranch, int fallbackLimit)
    {
        if (linkedTaskIds.Count == 0 || isGroupBranch)
            return linkedTaskIds;

        var inBranchName = ExtractTaskIds(sourceBranch, keyPrefix).Where(linkedTaskIds.Contains);
        var referenced = ExtractTaskReferences(keyPrefix, title, description);
        var named = linkedTaskIds.Where(referenced.Contains).Union(inBranchName).Distinct().ToList();
        if (named.Count > 0)
            return named;

        if (referenced.Overlaps(knownTaskIds))
            return [];

        return linkedTaskIds.Count <= Math.Max(fallbackLimit, 1) ? linkedTaskIds : [];
    }

    public static IList<int> ExtractMentionedTaskIds(string? text, string? keyPrefix)
    {
        var result = new List<int>();
        if (string.IsNullOrWhiteSpace(text))
            return result;

        foreach (Match m in Regex.Matches(text, @"(?<![\w#])#(\d{1,9})(?!\d)"))
            result.Add(int.Parse(m.Groups[1].Value));

        if (!string.IsNullOrWhiteSpace(keyPrefix))
        {
            var key = Regex.Escape(keyPrefix.Trim());
            foreach (Match m in Regex.Matches(text, $@"(?<![\w-]){key}-(\d{{1,9}})(?!\d)", RegexOptions.IgnoreCase))
                result.Add(int.Parse(m.Groups[1].Value));
        }

        return result.Distinct().ToList();
    }

    public static string? ValidateName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Branch name is required.";
        if (name.Length > 255)
            return "Branch name is too long.";
        if (name.Any(char.IsWhiteSpace) || Regex.IsMatch(name, @"[~^:?*\[\\]") || name.Contains("..") || name.Contains("@{") || name.Contains("//"))
            return "Branch name contains characters Git does not allow.";
        if (name.StartsWith('-') || name.StartsWith('/') || name.EndsWith('/') || name.EndsWith('.') || name.EndsWith(".lock"))
            return "Branch name has an invalid start or end.";
        return null;
    }

    #endregion
}
