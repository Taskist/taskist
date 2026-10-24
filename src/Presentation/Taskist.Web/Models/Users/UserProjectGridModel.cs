using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.Users;

public class UserProjectGridModel : BaseModel
{
	public string UserName { get; set; }

	public bool CanReportToDo { get; set; }

	public bool CanReportCR { get; set; }

	public bool CanReportBug { get; set; }

	public bool CanEdit { get; set; }

	public bool CanClose { get; set; }

	public bool CanReOpen { get; set; }

	public bool CanComment { get; set; }

	public bool CanViewOthersTask { get; set; }

	public bool CanEditOthersTask { get; set; }

	public bool CanAddWiki { get; set; }

	public bool CanDeleteTask { get; set; }

	public bool CanChangeProject { get; set; }

	public bool CanReviewMergeRequests { get; set; }

	public bool CanMergeMergeRequests { get; set; }
}
