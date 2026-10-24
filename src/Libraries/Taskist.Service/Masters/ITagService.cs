using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;

namespace Taskist.Service.Masters;

public interface ITagService
{
    Task<IPagedList<Tag>> GetPagedListAsync(string search = "", int pageIndex = 0, int pageSize = int.MaxValue,
       string sortColumn = "", string sortDirection = "");

    Task<IList<Tag>> GetAllActiveAsync(bool showDeleted = false);

    Task<Tag> GetByIdAsync(int id);

    Task<Tag> GetByNameAsync(string name);

    Task<Tag> GetOrCreateAsync(string name);

    Task InsertAsync(Tag entity);

    Task UpdateAsync(Tag entity);

    Task DeleteAsync(Tag entity);

    Task MergeAsync(int sourceId, int targetId);

    Task<List<int>> GetTagIdsByTaskAsync(int taskId);

    Task<Dictionary<int, string>> GetTagNamesByTaskIdsAsync(int[] taskIds);

    Task<int[]> ResolveTagIdsAsync(string[] tokens);

    Task SetTaskTagsAsync(int taskId, int[] tagIds);
}
