using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Taskist.Web.Models.Masters;

public class StatusModel : BaseModel
{
	public StatusModel()
	{
		AvailableProjects = [];
	}

	[LocalizedDisplayName("StatusModel.Name")]
	public string Name { get; set; }

	[LocalizedDisplayName("StatusModel.Description")]
	public string? Description { get; set; }

	[LocalizedDisplayName("StatusModel.Group")]
	public int GroupId { get; set; }

	public string GroupName { get; set; }

	[LocalizedDisplayName("StatusModel.Project")]
	public int? ProjectId { get; set; }

	public IList<SelectListItem> AvailableProjects { get; set; }

	[LocalizedDisplayName("StatusModel.TextColor")]
	public string? TextColor { get; set; }

	[LocalizedDisplayName("StatusModel.BackgroundColor")]
	public string? BackgroundColor { get; set; }

	[LocalizedDisplayName("StatusModel.IconClass")]
	public string? IconClass { get; set; }

	[LocalizedDisplayName("StatusModel.RequiredRootCauseForBug")]
	public bool RequiredRootCauseForBug { get; set; }

	[LocalizedDisplayName("StatusModel.RequiredRootCauseForCr")]
	public bool RequiredRootCauseForCr { get; set; }

	[LocalizedDisplayName("StatusModel.RequiredResolutionForBug")]
	public bool RequiredResolutionForBug { get; set; }

	[LocalizedDisplayName("StatusModel.RequiredResolutionForCr")]
	public bool RequiredResolutionForCr { get; set; }

	[LocalizedDisplayName("StatusModel.RequiredMrNumberForBug")]
	public bool RequiredMrNumberForBug { get; set; }

	[LocalizedDisplayName("StatusModel.RequiredMrNumberForCr")]
	public bool RequiredMrNumberForCr { get; set; }

	[LocalizedDisplayName("StatusModel.ExternalUse")]
	public bool ExternalUse { get; set; } = false;

	[LocalizedDisplayName("StatusModel.Active")]
	public bool Active { get; set; } = true;

	public bool SystemDefined { get; set; }
}
