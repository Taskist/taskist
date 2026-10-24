using Taskist.Core.Domain.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Core.Domain.Users;

namespace Taskist.Core.Domain.Wiki
{
    public class WikiDocument : BaseEntity, ISoftDeletedEntity, IAuditableEntity
    {
        public string Title { get; set; }

        public string ContentHtml { get; set; }

        public int CategoryId { get; set; }

        public int? ModuleId { get; set; }

        public int? SubModuleId { get; set; }

        public DateTime CreatedOn { get; set; }

        public int CreatedById { get; set; }

        public DateTime ModifiedOn { get; set; }

        public int ModifiedById { get; set; }

        public bool Deleted { get; set; }

        public virtual WikiCategory Category { get; set; }

        public virtual Module? Module { get; set; }

        public virtual SubModule? SubModule { get; set; }

        public virtual User CreatedBy { get; set; }

        public virtual User ModifiedBy { get; set; }
    }
}
