namespace Taskist.Web.Helpers.Common;

public class DiffRow
{
    public DiffRowKind Kind { get; set; }

    public int? OldLine { get; set; }

    public int? NewLine { get; set; }

    public string Text { get; set; } = string.Empty;
}
