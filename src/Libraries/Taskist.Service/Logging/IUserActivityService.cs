using Taskist.Core.Common;
using Taskist.Core.Domain.Common;
using Taskist.Core.Domain.Logging;
using Taskist.Core.Domain.Users;

namespace Taskist.Service.Logging;

public partial interface IUserActivityService
{
    Task<ActivityLog> InsertAsync(string systemName, string comment, BaseEntity entity = null);

    Task<ActivityLog> InsertAsync(User user, string systemName, string comment, BaseEntity entity = null);

    Task DeleteAsync(ActivityLog activityLog);

    Task<IPagedList<ActivityLog>> GetAllAsync(DateTime? createdOnFrom = null, DateTime? createdOnTo = null,
        int? userId = null, int? activityLogTypeId = null, string ipAddress = null, string entityName = null,
        int? entityId = null, int pageIndex = 0, int pageSize = int.MaxValue);

    Task<ActivityLog> GetByIdAsync(int activityLogId);

    Task ClearAllAsync();
}
