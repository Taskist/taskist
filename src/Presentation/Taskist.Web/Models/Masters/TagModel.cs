using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.Masters;

public class TagModel : BaseModel
{
    [LocalizedDisplayName("TagModel.Name")]
    public string Name { get; set; }

    [LocalizedDisplayName("TagModel.Active")]
    public bool Active { get; set; } = true;
}
