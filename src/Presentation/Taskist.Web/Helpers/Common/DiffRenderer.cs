using System.Text.RegularExpressions;

namespace Taskist.Web.Helpers.Common;

public static partial class DiffRenderer
{
    #region Methods

    public static List<DiffRow> Parse(string? diff)
    {
        var rows = new List<DiffRow>();
        if (string.IsNullOrEmpty(diff))
            return rows;

        int oldLine = 0, newLine = 0;
        foreach (var raw in diff.Replace("\r\n", "\n").Split('\n'))
        {
            if (raw.StartsWith("@@"))
            {
                var m = HunkRegex().Match(raw);
                if (m.Success)
                {
                    oldLine = int.Parse(m.Groups[1].Value);
                    newLine = int.Parse(m.Groups[2].Value);
                }
                rows.Add(new DiffRow { Kind = DiffRowKind.Hunk, Text = raw });
                continue;
            }

            if (raw.StartsWith('\\'))
            {
                rows.Add(new DiffRow { Kind = DiffRowKind.Note, Text = raw.TrimStart('\\', ' ') });
                continue;
            }

            if (raw.StartsWith('+'))
                rows.Add(new DiffRow { Kind = DiffRowKind.Added, NewLine = newLine++, Text = raw[1..] });
            else if (raw.StartsWith('-'))
                rows.Add(new DiffRow { Kind = DiffRowKind.Removed, OldLine = oldLine++, Text = raw[1..] });
            else if (raw.Length > 0 || oldLine > 0)
                rows.Add(new DiffRow { Kind = DiffRowKind.Context, OldLine = oldLine++, NewLine = newLine++, Text = raw.Length > 0 ? raw[1..] : string.Empty });
        }

        while (rows.Count > 0 && rows[^1].Kind == DiffRowKind.Context && string.IsNullOrEmpty(rows[^1].Text))
            rows.RemoveAt(rows.Count - 1);

        return rows;
    }

    public static (int Added, int Removed) Count(IEnumerable<DiffRow> rows)
    {
        var list = rows as IList<DiffRow> ?? rows.ToList();
        return (list.Count(r => r.Kind == DiffRowKind.Added), list.Count(r => r.Kind == DiffRowKind.Removed));
    }

    #endregion

    #region Utilities

    [GeneratedRegex(@"^@@ -(\d+)(?:,\d+)? \+(\d+)(?:,\d+)? @@")]
    private static partial Regex HunkRegex();

    #endregion
}
