using Taskist.Core.Enums;

namespace Taskist.Web.Models.Datatable;

public class ActionModel
{
	public ActionModel()
	{
		VisibleConditions = [];
		DeleteConfirmBox = false;
	}

	public string Id { get; set; }

	public string Title { get; set; }

	public string TitlePrefixDataParam { get; set; }

	public string TitleSufixDataParam { get; set; }

	public string DisplayReferenceParameter { get; set; }

	public bool DeleteConfirmBox { get; set; }

	public string DeleteConfirmBoxMsg { get; set; }

	public bool ConfirmBox { get; set; }

	public string ConfirmBoxMsg { get; set; }

	public string Url { get; set; }

	public string ReferenceParameter { get; set; }

	public string HideReferenceParameter { get; set; }

	public string Text { get; set; }

	public string AuxText { get; set; }

	public string Color { get; set; }

	public string Icon { get; set; }

	public string ModalSize { get; set; }

	public ButtonColor ButtonColor { get; set; }

	public string ButtonClass { get; set; }

	public NavigationType NavigationType { get; set; }

	public bool OpenInBlankPage { get; set; } = false;

	public HyperLinkType HyperLinkType { get; set; }

	public List<ActionCoditionModel> VisibleConditions { get; set; }
}
