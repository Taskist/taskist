using Taskist.Core.Enums;

namespace Taskist.Web.Models.Datatable;

public class ColumnConditionModel
{
	public ColumnConditionModel()
	{
		DataType = Taskist.Core.Enums.DataType.STRING;
		HideIfTrue = false;
	}

	public string Value { get; set; }

	public Operator Operator { get; set; }

	public Taskist.Core.Enums.DataType DataType { get; set; }

	public string TextColor { get; set; }

	public string BgColor { get; set; }

	public string Icon { get; set; }

	public string ReplaceWith { get; set; }

	public string ReferenceParameter { get; set; }

	public bool HideIfTrue { get; set; }
}
