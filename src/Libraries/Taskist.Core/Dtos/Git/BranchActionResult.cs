namespace Taskist.Core.Dtos.Git;

public class BranchActionResult
{
    public bool Success { get; set; }

    public string? Error { get; set; }

    public string? BranchName { get; set; }

    public bool NeedsConnection { get; set; }

    public List<int> LinkedTaskIds { get; set; } = [];

    public static BranchActionResult Fail(string error) => new() { Success = false, Error = error };

    public static BranchActionResult Connect(string error) => new() { Success = false, Error = error, NeedsConnection = true };
}
