using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.Masters
{
    public class CustomFieldRenderModel : BaseModel
    {
        public string Label { get; set; }

        public string ResourceKey { get; set; }

        public int FieldType { get; set; }

        public string? Options { get; set; }

        public bool Mandatory { get; set; }

        public int ColumnClass { get; set; }

        public int Placement { get; set; }

        public int ResolvedSlot { get; set; }

        public int Index { get; set; }

        public string Value { get; set; } = "";
    }
}
