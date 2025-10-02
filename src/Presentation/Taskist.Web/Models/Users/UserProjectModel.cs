using Microsoft.AspNetCore.Mvc.Rendering;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;

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

	[LocalizedDisplayName("UserProjectModel.CanReport")]
	public bool CanReport { get; set; }

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

	public IList<SelectListItem> AvailableUsers { get; set; }
}

public class UserProjectGridModel : BaseModel
{
	public string UserName { get; set; }

	public bool CanReport { get; set; }

	public bool CanEdit { get; set; }

	public bool CanClose { get; set; }

	public bool CanReOpen { get; set; }

	public bool CanComment { get; set; }
}