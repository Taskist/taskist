using Taskist.Core.Common;
using Taskist.Core.Domain.Wiki;

namespace Taskist.Service.Wiki;

public interface IWikiCategoryService
{
    Task<IPagedList<WikiCategory>> GetPagedListAsync(string search = "", int pageIndex = 0, int pageSize = int.MaxValue,
       string sortColumn = "", string sortDirection = "");

    Task<IList<WikiCategory>> GetAllActiveAsync();

    Task<IList<WikiCategory>> GetAllParentAsync(int projectId);

    Task<IList<WikiCategory>> GetAllByParentCategoryIdAsync(int parentCategoryId, int projectId);

    Task<WikiCategory> GetByIdAsync(int id);

    Task<WikiCategory> GetByNameAsync(string name);

    Task InsertAsync(WikiCategory entity);

    Task InsertAsync(List<WikiCategory> entities);

    Task UpdateAsync(WikiCategory entity);

    Task DeleteAsync(WikiCategory entity);
}
