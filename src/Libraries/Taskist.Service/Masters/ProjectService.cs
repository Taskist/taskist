using Taskist.Core.Common;
using Taskist.Core.Domain.Users;
using Taskist.Core.Domain.Masters;
using Taskist.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;
using System.Linq.Dynamic.Core;

namespace Taskist.Service.Masters;

public class ProjectService : IProjectService
{
    #region Fields

    protected readonly IRepository<Project> _projectRepository;
    protected readonly IRepository<UserProjectMap> _projectMemberMapRepository;

    #endregion

    #region Ctor
    public ProjectService(IRepository<Project> projectRepository,
        IRepository<UserProjectMap> projectMemberMapRepository)
    {
        _projectRepository = projectRepository;
        _projectMemberMapRepository = projectMemberMapRepository;
    }
    #endregion

    #region Methods

    public async Task<IPagedList<Project>> GetPagedListAsync(string search = "", int pageIndex = 0,
    int pageSize = int.MaxValue, string sortColumn = "", string sortDirection = "")
    {
        return await _projectRepository.GetAllPagedAsync(query =>
        {
            query = query.Where(x => !x.Deleted);
            query = query.OrderBy($"{sortColumn} {sortDirection}");

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
    }

    public async Task UpdateMemberAsync(UserProjectMap entity)
    {

        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _projectMemberMapRepository.UpdateAsync(entity);
    }

    public async Task DeleteMemberAsync(UserProjectMap entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _projectMemberMapRepository.DeleteAsync(entity);
    }

    #endregion
}
