using Taskist.Core.Common;
using Taskist.Core.Domain.WorkItems;

namespace Taskist.Service.WorkItems;

public interface IReleaseService
{
    Task<IPagedList<Release>> GetPagedListAsync(int lastAccessedProjectId, int[] accessiableProjects, string search = "",
        int pageIndex = 0, int pageSize = int.MaxValue, string sortColumn = "", string sortDirection = "",
        int[]? statuses = null, int[]? releaseTypes = null, DateOnly? fromDate = null, DateOnly? toDate = null);

    Task<IList<Release>> GetAllAsync(bool showDeleted = false);

    Task<IList<Release>> GetAllActiveAsync(bool showDeleted = false);

    Task<IList<Release>> GetAllActiveByProjectAsync(int projectId);

    Task<IList<Release>> GetAllByProjectAsync(int projectId);

    Task<IList<Release>> GetAllActiveForAllProjectsAsync(IList<int> projectIds);

    Task<IList<Release>> GetAllForAnyProjectAsync(IList<int> projectIds);

    Task<bool> IsAvailableForAllProjectsAsync(int releaseId, IList<int> projectIds);

    Task<IList<ReleaseProject>> GetProjectsAsync(int releaseId);

    Task SaveProjectsAsync(int releaseId, IList<int> projectIds);

    Task<Release> GetByIdAsync(int id);

    Task<Release> GetByNameAsync(string name);

    Task<bool> NameExistsInScopeAsync(string name, IList<int> projectIds, int excludeReleaseId = 0);

    Task InsertAsync(Release entity);

    Task InsertAsync(IList<Release> entities);

    Task UpdateAsync(Release entity);

    Task DeleteAsync(Release entity);
}
