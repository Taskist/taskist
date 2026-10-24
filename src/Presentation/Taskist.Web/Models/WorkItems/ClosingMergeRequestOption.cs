namespace Taskist.Web.Models.WorkItems;

public class ClosingMergeRequestOption
{
    public int Iid { get; set; }

    public int RepositoryId { get; set; }

    public string? RepositoryName { get; set; }

    public string RefPrefix { get; set; } = "!";

    public string Title { get; set; }

    public string? State { get; set; }

    public string? SourceBranch { get; set; }

    public string? Author { get; set; }

    public bool Linked { get; set; }
}
