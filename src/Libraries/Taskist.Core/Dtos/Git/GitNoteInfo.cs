namespace Taskist.Core.Dtos.Git;

public class GitNoteInfo
{
    public int MergeRequestIid { get; set; }

    public string? MergeRequestTitle { get; set; }

    public string? MergeRequestUrl { get; set; }

    public string? MergeRequestAuthorUserName { get; set; }

    public string? Body { get; set; }

    public string? Url { get; set; }

    public bool System { get; set; }
}
