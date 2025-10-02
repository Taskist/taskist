namespace Taskist.Web.Models.Datatable;

public class DataSourceRequest
{
    public int Draw { get; set; }

    public int Start { get; set; }

    public int Length { get; set; }

    public int SortColumn { get; set; }
}