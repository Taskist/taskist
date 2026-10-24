namespace Taskist.Core.Dtos.Reports;

public class StageDurationDto
{
    public int MetricId { get; set; }

    public string Metric { get; set; }

    public string FromStatus { get; set; }

    public string ToStatus { get; set; }

    public int Count { get; set; }

    public double AvgDays { get; set; }

    public double MedianDays { get; set; }

    public double MinDays { get; set; }

    public double MaxDays { get; set; }

    public string AvgText { get; set; }

    public string MedianText { get; set; }

    public string MinText { get; set; }

    public string MaxText { get; set; }
}
