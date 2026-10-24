namespace Taskist.Core.Dtos.Git;

public class ReviewActionResult
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public static ReviewActionResult Ok(string message) => new() { Success = true, Message = message };

    public static ReviewActionResult Fail(string message) => new() { Success = false, Message = message };
}
