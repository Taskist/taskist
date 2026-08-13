using Taskist.Service.Common;
using Microsoft.EntityFrameworkCore;
using Taskist.Core.Caching;
using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Core.Domain.Users;
using Taskist.Data.Repository;
using Taskist.Data.Extensions;

namespace Taskist.Service.Masters;

public class ProjectService : IProjectService
{
    #region Fields

    protected readonly IRepository<Project> _projectRepository;
    protected readonly IRepository<UserProjectMap> _projectMemberMapRepository;
    protected readonly ICacheManager _cacheManager;

    #endregion

    #region Ctor
    public ProjectService(IRepository<Project> projectRepository,
        IRepository<UserProjectMap> projectMemberMapRepository,
        ICacheManager cacheManager)
    {
        _projectRepository = projectRepository;
        _projectMemberMapRepository = projectMemberMapRepository;
        _cacheManager = cacheManager;
    }
    #endregion

    #region Methods

    public async Task<IPagedList<Project>> GetPagedListAsync(string search = "", int pageIndex = 0,
    int pageSize = int.MaxValue, string sortColumn = "", string sortDirection = "")
    {
        return await _projectRepository.GetAllPagedAsync(query =>
        {
            query = query.Where(x => !x.Deleted);
            query = query.OrderBySafe(sortColumn, sortDirection);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.Name.Contains(search));

            return query;
        }, pageIndex, pageSize);
    }

    public async Task<IList<Project>> GetAllAsync(bool showDeleted = false)
    {
        return await _projectRepository.GetAllAsync(includeDeleted: showDeleted);
    }

    public async Task<IList<Project>> GetAllActiveAsync(bool showDeleted = false)
    {
        return await _projectRepository.GetAllAsync(q => q.Where(x => x.Active), showDeleted);
    }

    public async Task<Project> GetByIdAsync(int id)
    {
        return id == 0 ? null : await _projectRepository.GetByIdAsync(id);
    }

    public async Task<Project> GetByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var query = from c in _projectRepository.Table
                    orderby c.Id
                    where !c.Deleted && c.Name == name
                    select c;
        return await query.FirstOrDefaultAsync();
    }

    public async Task<UserProjectMap> GetMappingForUserAsync(int projectId, int userId)
    {
        return await _projectMemberMapRepository.Table.Where(x => x.ProjectId == projectId && x.UserId == userId)
            .FirstOrDefaultAsync();
    }

    public async Task<IList<Project>> GetAllAccessibleAsync(int userId)
    {
        var query = from c in _projectMemberMapRepository.Table
                    where !c.Project.Deleted && c.Project.Active && c.UserId == userId && c.CanReport
                    select c.Project;

        return await query.ToListAsync();
    }

    public async Task InsertAsync(Project entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _projectRepository.InsertAsync(entity);
    }

    public async Task InsertAsync(IList<Project> entities)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        await _projectRepository.InsertAsync(entities);
    }

    public async Task UpdateAsync(Project entity)
    {

        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _projectRepository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(Project entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _projectRepository.DeleteAsync(entity);
    }

    #endregion

    #region Members Mapping

    public async Task<IPagedList<UserProjectMap>> GetPagedListMembersAsync(int projectId, string search = "", int pageIndex = 0,
        int pageSize = int.MaxValue)
    {
        return await _projectMemberMapRepository.GetAllPagedAsync(query =>
        {
            query = query.Where(x => x.ProjectId == projectId);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.User.Name.Contains(search) ||
                c.Project.Name.Contains(search));

            return query;
        }, pageIndex, pageSize);
    }

    public async Task<UserProjectMap> GetMemberByIdAsync(int id)
    {
        return id == 0 ? null : await _projectMemberMapRepository.GetByIdAsync(id);
    }

    public async Task<UserProjectMap> GetMemberByIdAndProjectAsync(int userId, int projectId)
    {
        var query = _projectMemberMapRepository.Table.AsNoTracking().Where(x => x.UserId == userId && x.ProjectId == projectId);
        return await query.FirstOrDefaultAsync();
    }

    public async Task InsertMemberAsync(UserProjectMap entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _projectMemberMapRepository.InsertAsync(entity);

        await InvalidateAccessibleProjectsAsync(entity.UserId);
    }

    public async Task UpdateMemberAsync(UserProjectMap entity)
    {

        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _projectMemberMapRepository.UpdateAsync(entity);

        await InvalidateAccessibleProjectsAsync(entity.UserId);
    }

    public async Task DeleteMemberAsync(UserProjectMap entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var userId = entity.UserId;

        await _projectMemberMapRepository.DeleteAsync(entity);

        await InvalidateAccessibleProjectsAsync(userId);
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Drops the cached accessible-project list so membership changes take effect immediately.
    /// </summary>
    protected async Task InvalidateAccessibleProjectsAsync(int userId)
    {
        await _cacheManager.RemoveAsync(string.Format(ServiceConstant.AccessibleProjectCacheKey, userId));
    }

    #endregion
}
