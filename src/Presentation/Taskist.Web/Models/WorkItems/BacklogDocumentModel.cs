using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.WorkItems;

public class BacklogDocumentGridModel : BasePageModel
{
	public string Name { get; set; }

	public string ContentType { get; set; }

	public string Extension { get; set; }

	public long FileSize { get; set; }
}
