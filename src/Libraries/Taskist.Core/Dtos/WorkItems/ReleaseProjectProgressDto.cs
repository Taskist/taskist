namespace Taskist.Core.Dtos.WorkItems;

public class ReleaseProjectProgressDto
{
    public int ReleaseId { get; set; }

    public int ProjectId { get; set; }

    public string Project { get; set; }

    public int Total { get; set; }

    public int Completed { get; set; }
}
