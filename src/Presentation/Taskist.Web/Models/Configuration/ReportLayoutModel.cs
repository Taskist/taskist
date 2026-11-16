namespace Taskist.Web.Models.Configuration;

public class ReportLayoutModel
{
    public string Key { get; set; }

    public string Name { get; set; }

    public bool EditTitles { get; set; }

    public List<ReportColumnItemModel> Columns { get; set; } = [];
}
