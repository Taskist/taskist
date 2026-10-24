namespace Taskist.Core.Dtos.Reports;

public class ResourceLoadDto
{
    public int UserId { get; set; }

    public string Name { get; set; }

    public string Role { get; set; }

    public string Projects { get; set; }

    public int Open { get; set; }

    public int Overdue { get; set; }

    public int DueSoon { get; set; }

    public int Reopened { get; set; }

    public int CriticalHigh { get; set; }

    public double Score { get; set; }

    public string Band { get; set; }
}
