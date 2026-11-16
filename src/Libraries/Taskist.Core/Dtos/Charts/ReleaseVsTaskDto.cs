namespace Taskist.Core.Dtos.Charts;

public class ReleaseVsTaskDto
{
    public string ReleaseVersion { get; set; }

    public int OpenTasks { get; set; }

    public int ClosedTasks { get; set; }
}
