using Taskist.Core.Common;
using Taskist.Core.Domain.Users;
using Taskist.Core.Domain.Masters;

namespace Taskist.Service.Masters;

public interface IProjectService
{
    Task<IPagedList<Project>> GetPagedListAsync(string search = "", int pageIndex = 0, int pageSize = int.MaxValue,
       string sortColumn = "", string sortDirection = "");

    Task<IList<Project>> GetAllAsync(bool showDeleted = false);

    Task<IList<Project>> GetAllActiveAsync(bool showDeleted = false);

    Task<Project> GetByIdAsync(int id);

    Task<Project> GetByNameAsync(string name);

    Task<UserProjectMap> GetMappingForUserAsync(int projectId, int userId);

    Task InsertAsync(Project entity);

    Task InsertAsync(IList<Project> entities);

    Task UpdateAsync(Project entity);

    Task DeleteAsync(Project entity);

    #region Members Mapping

    Task<IPagedList<UserProjectMap>> GetPagedListMembersAsync(int projectId, string search = "", int pageIndex = 0,
        int pageSize = int.MaxValue);

    Task<UserProjectMap> GetMemberByIdAsync(int id);

    Task<UserProjectMap> GetMemberByIdAndProjectAsync(int userId, int projectId);

    Task InsertMemberAsync(UserProjectMap entity);

    Task UpdateMemberAsync(UserProjectMap entity);

    Task DeleteMemberAsync(UserProjectMap entity);

    #endregion
}
