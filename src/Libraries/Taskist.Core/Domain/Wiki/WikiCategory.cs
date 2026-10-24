using Taskist.Core.Domain.Common;
using Taskist.Core.Domain.Masters;

namespace Taskist.Core.Domain.Wiki
{
    public class WikiCategory : BaseEntity, ISoftDeletedEntity, IAuditableEntity
    {
        public string Name { get; set; }

        public string? Description { get; set; }

        public int? ParentCategoryId { get; set; }

        public int ProjectId { get; set; }

        public bool Deleted { get; set; }

        public bool Active { get; set; }

        public virtual WikiCategory? ParentCategory { get; set; }

        public virtual Project Project { get; set; }
    }
}
