using Taskist.Core.Domain.Common;

namespace Taskist.Core.Domain.Masters
{
    public class CustomField : BaseEntity
    {
        public int ProjectId { get; set; }

        public string Label { get; set; }

        public string ResourceKey { get; set; }

        public int FieldType { get; set; }

        public string? Options { get; set; }

        public bool Mandatory { get; set; }

        public int ColumnClass { get; set; }

        public int Placement { get; set; }

        public int DisplayOrder { get; set; }

        public virtual Project Project { get; set; }
    }
}
