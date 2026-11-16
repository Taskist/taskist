namespace Taskist.Core.Dtos.Git;

public class MyCodeDto
{
    public string State { get; set; } = MyCodeState.Ok;

    public string? Error { get; set; }

    public List<MyCodeMergeRequestDto> Authored { get; set; } = [];

    public List<MyCodeMergeRequestDto> Reviewing { get; set; } = [];

    public List<MyCodeBranchDto> Branches { get; set; } = [];

    public DateTime LoadedOn { get; set; } = DateTime.UtcNow;

    public bool IsEmpty => Authored.Count == 0 && Reviewing.Count == 0 && Branches.Count == 0;
}
