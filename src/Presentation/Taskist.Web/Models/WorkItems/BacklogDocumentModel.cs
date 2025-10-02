using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.WorkItems;

public class BacklogDocumentGridModel : BaseModel
{
	public string Name { get; set; }

	public string ContentType { get; set; }
}
