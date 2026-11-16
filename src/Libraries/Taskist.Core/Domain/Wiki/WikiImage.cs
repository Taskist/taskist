using Taskist.Core.Domain.Common;

namespace Taskist.Core.Domain.Wiki;

public class WikiImage : BaseEntity
{
    public int DocumentId { get; set; }

    public int ProjectId { get; set; }

    public int WikiDocumentId { get; set; }

    public int UploadedById { get; set; }

    public DateTime CreatedOn { get; set; }
}
