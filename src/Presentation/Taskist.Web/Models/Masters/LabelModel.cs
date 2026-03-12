using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.Masters
{
    public class LabelModel : BasePageModel
    {
        [LocalizedDisplayName("LabelModel.Name")]
        public string Name { get; set; }

        [LocalizedDisplayName("LabelModel.Color")]
        public string Color { get; set; }

        [LocalizedDisplayName("LabelModel.Description")]
        public string? Description { get; set; }

        [LocalizedDisplayName("StatusModel.Active")]
        public bool Active { get; set; } = true;

        [LocalizedDisplayName("LabelModel.Deleted")]
        public string? Deleted { get; set; }



      

        
    }
}
