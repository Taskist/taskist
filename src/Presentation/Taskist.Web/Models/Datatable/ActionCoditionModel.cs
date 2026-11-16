using Taskist.Core.Enums;

namespace Taskist.Web.Models.Datatable;

public class ActionCoditionModel
{
	public ActionCoditionModel()
	{
		DataType = Taskist.Core.Enums.DataType.STRING;
	}

	public string Value { get; set; }

	public Operator Operator { get; set; }

	public Taskist.Core.Enums.DataType DataType { get; set; }

	public string ReferenceParameter { get; set; }
}
