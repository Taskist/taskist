using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.Masters
{
    public class CustomFieldGridModel : BaseModel
    {
        public string Label { get; set; }

        public string ResourceKey { get; set; }

        public string FieldType { get; set; }

        public bool Mandatory { get; set; }

        public string ColumnClass { get; set; }

        public string Placement { get; set; }

        public string DisplayOrder { get; set; }
    }
}
