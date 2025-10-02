using Taskist.Core.Domain.Common;
using Taskist.Core.Domain.Masters;

namespace Taskist.Core.Domain.WorkItems
{
    public class BacklogCustomFieldValue : BaseEntity
    {
        public int BacklogId { get; set; }

        public int CustomFieldId { get; set; }

        public string? Value { get; set; }

        public virtual Backlog Backlog { get; set; }

        public virtual CustomField CustomField { get; set; }
    }
}
