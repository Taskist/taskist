namespace Taskist.Core.Dtos.Git;

public class IssueSyncResult
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public static IssueSyncResult Ok(string message) => new() { Success = true, Message = message };

    public static IssueSyncResult Fail(string message) => new() { Success = false, Message = message };
}
