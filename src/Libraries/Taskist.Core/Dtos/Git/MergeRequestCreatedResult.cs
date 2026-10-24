namespace Taskist.Core.Dtos.Git;

public class MergeRequestCreatedResult
{
    public bool Success { get; set; }

    public string? Error { get; set; }

    public bool NeedsConnection { get; set; }

    public int Iid { get; set; }

    public string? Url { get; set; }

    public int MovedTasks { get; set; }

    public string RefPrefix { get; set; } = "!";

    public static MergeRequestCreatedResult Fail(string error, bool needsConnection = false) => new() { Error = error, NeedsConnection = needsConnection };
}
