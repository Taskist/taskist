namespace Taskist.Web.Models.Common;

public class FileTilesModel
{
    public IList<FileTileModel> Items { get; set; } = [];

    public bool AllowDelete { get; set; }

    public string EmptyText { get; set; } = "No files uploaded yet.";
}
