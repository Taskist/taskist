namespace Taskist.Web.Models.Datatable;

public class ColumnModel
{
	public ColumnModel()
	{
		Conditions = [];
		Sorting = true;
		Capitalize = false;
		MaxLength = 100;
		IsDate = false;
		IsRating = false;
		IsColor = false;
	}

	public string Title { get; set; }

	public string DataColumn { get; set; }

	public bool Sorting { get; set; }

	public bool Capitalize { get; set; }

	public int MaxLength { get; set; }

	public bool IsDate { get; set; }

	public bool IsRating { get; set; }

	public bool IsColor { get; set; }

	public bool IsIconClass { get; set; }

	public bool IsDayCount { get; set; }

	public List<ColumnConditionModel> Conditions { get; set; }
}