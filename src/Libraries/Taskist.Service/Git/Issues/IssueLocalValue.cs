namespace Taskist.Service.Git.Issues;

public class IssueLocalValue
{
    public string Hash { get; init; } = string.Empty;

    public string? RemoteForm { get; init; }

    public bool Exportable { get; init; } = true;
}
