using Taskist.Core.Domain.Common;
using Taskist.Core.Domain.Users;

namespace Taskist.Core.Domain.Wiki
{
    public class WikiDocumentVersion : BaseEntity, IAuditableEntity
    {
        public int WikiDocumentId { get; set; }

        public string Title { get; set; }

        public string ContentHtml { get; set; }

        public DateTime CreatedOn { get; set; }

        public int ModifiedById { get; set; }

        public string? Note { get; set; }

        public virtual WikiDocument WikiDocument { get; set; }

        public virtual User ModifiedBy { get; set; }
    }
}
