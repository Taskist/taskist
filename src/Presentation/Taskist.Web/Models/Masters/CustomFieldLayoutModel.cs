using Taskist.Core.Common;
using Taskist.Core.Enums;

namespace Taskist.Web.Models.Masters
{
    public class CustomFieldLayoutModel
    {
        public CustomFieldLayoutModel()
        {
            Fields = [];
        }

        public bool? CanUpdate { get; set; }

        public List<CustomFieldRenderModel> Fields { get; set; }

        public CustomFieldsViewModel For(BacklogFormSlot slot)
        {
            return new CustomFieldsViewModel
            {
                InsideRow = CustomFieldLayout.RendersInsideStatusRow(slot),
                CanUpdate = CanUpdate == true,
                CustomFields = Fields.Where(x => x.ResolvedSlot == (int)slot).ToList()
            };
        }
    }
}
