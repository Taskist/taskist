using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Taskist.Web.Models.Masters;

public class TagMergeModel : BaseModel
{
    public TagMergeModel()
    {
        AvailableTargets = [];
    }

    public int SourceId { get; set; }

    public string SourceName { get; set; }

    [LocalizedDisplayName("TagModel.MergeTarget")]
    public int TargetId { get; set; }

    public IList<SelectListItem> AvailableTargets { get; set; }
}
