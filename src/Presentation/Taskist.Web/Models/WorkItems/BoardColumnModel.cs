namespace Taskist.Web.Models.WorkItems;

public class BoardColumnModel
{
	public int StatusId { get; set; }

	public string Name { get; set; }

	public string TextColor { get; set; }

	public string BackgroundColor { get; set; }

	public string IconClass { get; set; }

	public int Total { get; set; }

	public int Loaded { get; set; }

	public List<BoardCardModel> Cards { get; set; } = [];
}
