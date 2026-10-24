using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Taskist.Web.Models.Users;

public class UserProjectModel : BaseModel
{
	public UserProjectModel()
	{
		AvailableUsers = [];
	}

	public int ProjectId { get; set; }

	[LocalizedDisplayName("UserProjectModel.User")]
	public int UserId { get; set; }

	public string UserName { get; set; }

	[LocalizedDisplayName("UserProjectModel.CanReportToDo")]
	public bool CanReportToDo { get; set; }

	[LocalizedDisplayName("UserProjectModel.CanReportCR")]
	public bool CanReportCR { get; set; }

	[LocalizedDisplayName("UserProjectModel.CanReportBug")]
	public bool CanReportBug { get; set; }

	[LocalizedDisplayName("UserProjectModel.CanEdit")]
	public bool CanEdit { get; set; }

	[LocalizedDisplayName("UserProjectModel.CanClose")]
	public bool CanClose { get; set; }

	[LocalizedDisplayName("UserProjectModel.CanReOpen")]
	public bool CanReOpen { get; set; }

	[LocalizedDisplayName("UserProjectModel.CanComment")]
	public bool CanComment { get; set; }

	[LocalizedDisplayName("UserProjectModel.CanViewOthersTask")]
	public bool CanViewOthersTask { get; set; }

	[LocalizedDisplayName("UserProjectModel.CanEditOthersTask")]
	public bool CanEditOthersTask { get; set; }

	[LocalizedDisplayName("UserProjectModel.CanAddWiki")]
	public bool CanAddWiki { get; set; }

	[LocalizedDisplayName("UserProjectModel.CanDeleteTask")]
	public bool CanDeleteTask { get; set; }

	[LocalizedDisplayName("UserProjectModel.CanChangeProject")]
	public bool CanChangeProject { get; set; }

	[LocalizedDisplayName("UserProjectModel.CanReviewMergeRequests")]
	public bool CanReviewMergeRequests { get; set; }

	[LocalizedDisplayName("UserProjectModel.CanMergeMergeRequests")]
	public bool CanMergeMergeRequests { get; set; }

	public IList<SelectListItem> AvailableUsers { get; set; }
}