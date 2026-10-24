namespace Taskist.Web.Models.Common;

public class FileTileModel
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string? ContentType { get; set; }

    public long FileSize { get; set; }

    public string? Extension { get; set; }

    public string Url { get; set; }
}
