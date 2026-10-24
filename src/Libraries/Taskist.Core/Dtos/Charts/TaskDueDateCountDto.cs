namespace Taskist.Core.Dtos.Charts;

public class TaskDueDateCountDto
{
    public int Today { get; set; }

    public int Tomorrow { get; set; }

    public int NextSevenDays { get; set; }

    public int Overdue { get; set; }

    public int NoDueDate { get; set; }
}
