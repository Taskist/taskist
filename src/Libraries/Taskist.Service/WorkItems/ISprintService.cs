using Taskist.Core.Common;
using Taskist.Core.Domain.WorkItems;

namespace Taskist.Service.WorkItems;

public interface ISprintService
{
	Task<IPagedList<Sprint>> GetPagedListAsync(string search = "", int pageIndex = 0, int pageSize = int.MaxValue,
	   string sortColumn = "", string sortDirection = "");

	Task<IList<Sprint>> GetAllAsync(bool showDeleted = false);

	Task<IList<Sprint>> GetAllActiveAsync(bool showDeleted = false);

	Task<IList<Sprint>> GetAllActiveByProjectAsync(int projectId);

	Task<IList<Sprint>> GetSprintBetweenDateAsync(int projectId, DateOnly startDate, DateOnly endDate);

	Task<Sprint> GetByIdAsync(int id);

	Task<Sprint> GetByNameAsync(string name);

	Task InsertAsync(Sprint entity);

	Task InsertAsync(IList<Sprint> entities);

	Task UpdateAsync(Sprint entity);

	Task DeleteAsync(Sprint entity);
}
