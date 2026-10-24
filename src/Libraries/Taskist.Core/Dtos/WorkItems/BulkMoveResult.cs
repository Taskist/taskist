namespace Taskist.Core.Dtos.WorkItems;

public class BulkMoveResult
{
    public int Moved { get; set; }

    public string? Error { get; set; }

    public int AddedSubTasks { get; set; }

    public List<SkippedTask> Skipped { get; set; } = [];

    public List<StayingSubTask> StayingClosedSubTasks { get; set; } = [];

    public class SkippedTask
    {
        public int TaskId { get; set; }

        public string Reason { get; set; }
    }

    public class StayingSubTask
    {
        public int ParentId { get; set; }

        public int TaskId { get; set; }
    }
}
