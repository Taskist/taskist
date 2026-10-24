namespace Taskist.Web.Models.Dashboard;

public class CardWithCountsModel
{
    public List<CardModel> Cards { get; set; } = [];

    public int SelectedProjectId { get; set; }
}
