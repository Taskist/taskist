namespace Taskist.Core.Dtos.Reports;

public class ReportBuilderDto
{
    public List<string> AvailableFields { get; set; } =
        [
            "Title", "CreatedOn", "Status.Name", "Severity.Name", "Assignee.Name"
        ];

    public List<string> SelectedFields { get; set; } = [];

    public string SortBy { get; set; }

    public bool SortDescending { get; set; }

    public List<ReportConditionDto> Conditions { get; set; } = [];

    public List<SortOrderDto> SortOrders { get; set; } = [];

    public List<Dictionary<string, object>> Results { get; set; } = [];

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}
