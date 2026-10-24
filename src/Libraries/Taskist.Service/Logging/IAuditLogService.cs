using Taskist.Core.Common;
using Taskist.Core.Domain.Logging;

namespace Taskist.Service.Logging;

public interface IAuditLogService
{
    Task<IPagedList<AuditLog>> GetAllAsync(DateTime? createdOnFrom = null, DateTime? createdOnTo = null,
        int? userId = null, string entityName = null, string action = null,
        int pageIndex = 0, int pageSize = int.MaxValue);

    Task<AuditLog> GetByIdAsync(int id);

    Task<int> DeleteOlderThanAsync(DateTime cutoffUtc);
}
