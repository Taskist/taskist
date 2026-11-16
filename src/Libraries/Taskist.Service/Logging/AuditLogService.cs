using Taskist.Core.Common;
using Taskist.Core.Domain.Logging;
using Taskist.Data.Repository;

namespace Taskist.Service.Logging;

public class AuditLogService : IAuditLogService
{
    protected readonly IRepository<AuditLog> _auditLogRepository;

    public AuditLogService(IRepository<AuditLog> auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<IPagedList<AuditLog>> GetAllAsync(DateTime? createdOnFrom = null, DateTime? createdOnTo = null,
        int? userId = null, string entityName = null, string action = null,
        int pageIndex = 0, int pageSize = int.MaxValue)
    {
        return await _auditLogRepository.GetAllPagedAsync(query =>
        {
            if (createdOnFrom.HasValue)
                query = query.Where(x => x.CreatedOnUtc >= createdOnFrom.Value);
            if (createdOnTo.HasValue)
                query = query.Where(x => x.CreatedOnUtc <= createdOnTo.Value);
            if (userId.HasValue && userId.Value > 0)
                query = query.Where(x => x.UserId == userId.Value);
            if (!string.IsNullOrEmpty(entityName))
                query = query.Where(x => x.EntityName == entityName);
            if (!string.IsNullOrEmpty(action))
                query = query.Where(x => x.Action == action);

            return query.OrderByDescending(x => x.CreatedOnUtc).ThenByDescending(x => x.Id);
        }, pageIndex, pageSize);
    }

    public async Task<AuditLog> GetByIdAsync(int id)
    {
        return await _auditLogRepository.GetByIdAsync(id);
    }

    public async Task<int> DeleteOlderThanAsync(DateTime cutoffUtc)
    {
        var old = await _auditLogRepository.GetAllAsync(query => query.Where(x => x.CreatedOnUtc < cutoffUtc));
        if (old.Count == 0)
            return 0;

        await _auditLogRepository.DeleteAsync(old);
        return old.Count;
    }
}
