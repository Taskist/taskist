namespace Taskist.Core.Domain.Common;

public interface ISoftDeletedEntity
{
    bool Deleted { get; set; }
}
