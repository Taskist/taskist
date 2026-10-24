using Taskist.Core.Extensions;
using Taskist.Core.Enums;
using Taskist.Core.Common;
using Taskist.Core.Domain.WorkItems;
using Taskist.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;
using System.Linq.Dynamic.Core;

namespace Taskist.Service.WorkItems;

public class ReleaseService : IReleaseService
{
    #region Constants

    public const int ReleaseOverdueFilter = -1;

    #endregion

    #region Fields

    protected readonly IRepository<Release> _releaseRepository;
    protected readonly IRepository<ReleaseProject> _releaseProjectRepository;

    #endregion

    #region Ctor
    public ReleaseService(IRepository<Release> releaseRepository,
        IRepository<ReleaseProject> releaseProjectRepository)
    {
        _releaseRepository = releaseRepository;
        _releaseProjectRepository = releaseProjectRepository;
    }
    #endregion

    #region Methods

    public async Task<IPagedList<Release>> GetPagedListAsync(int lastAccessedProjectId,
        int[] accessiableProjects, string search = "", int pageIndex = 0,
        int pageSize = int.MaxValue, string sortColumn = "", string sortDirection = "",
        int[]? statuses = null, int[]? releaseTypes = null, DateOnly? fromDate = null, DateOnly? toDate = null)
    {
        return await _releaseRepository.GetAllPagedAsync(query =>
        {
            query = query.Where(x => !x.Deleted)
                .Include(x => x.ReleaseProjects).ThenInclude(p => p.Project);

            if (lastAccessedProjectId > 0)
                query = query.Where(x => !x.ReleaseProjects.Any() || x.ReleaseProjects.Any(p => p.ProjectId == lastAccessedProjectId));
            else if (accessiableProjects != null && accessiableProjects.Length > 0)
                query = query.Where(x => !x.ReleaseProjects.Any() || x.ReleaseProjects.Any(p => accessiableProjects.Contains(p.ProjectId)));

            if (statuses != null && statuses.Length > 0)
            {
                var includeOverdue = statuses.Contains(ReleaseOverdueFilter);
                var realStatuses = statuses.Where(s => s > 0).ToArray();
                var planning = ReleaseStatus.Planning.ToInt();
                var today = DateOnly.FromDateTime(DateTime.Today);

                query = query.Where(x => realStatuses.Contains(x.Status)
                    || (includeOverdue && x.Status == planning && x.ReleaseDate < today));
            }

            if (releaseTypes != null && releaseTypes.Length > 0)
                query = query.Where(x => releaseTypes.Contains(x.ReleaseType));

            if (fromDate.HasValue)
                query = query.Where(x => x.ReleaseDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(x => x.ReleaseDate <= toDate.Value);

            query = query.ApplySafeSort(sortColumn, sortDirection);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.Name.Contains(search) || (c.Description != null && c.Description.Contains(search)));

            return query;
        }, pageIndex, pageSize);
    }
    public async Task<IList<Release>> GetAllAsync(bool showDeleted = false)
    {
        return await _releaseRepository.GetAllAsync(includeDeleted: showDeleted);
    }

    public async Task<IList<Release>> GetAllActiveAsync(bool showDeleted = false)
    {
        return await _releaseRepository.GetAllAsync(q => q.Where(x => !x.Deleted), showDeleted);
    }

    public async Task<IList<Release>> GetAllActiveByProjectAsync(int projectId)
    {
        return await _releaseRepository.GetAllAsync(q => q.Where(x =>
            (!x.ReleaseProjects.Any() || x.ReleaseProjects.Any(p => p.ProjectId == projectId)) &&
            !x.Deleted && x.Status == ReleaseStatus.Planning.ToInt()));
    }

    public async Task<IList<Release>> GetAllByProjectAsync(int projectId)
    {
        return await _releaseRepository.GetAllAsync(q => q.Where(x =>
            !x.ReleaseProjects.Any() || x.ReleaseProjects.Any(p => p.ProjectId == projectId)));
    }

    public async Task<IList<Release>> GetAllActiveForAllProjectsAsync(IList<int> projectIds)
    {
        var ids = (projectIds ?? []).Distinct().ToList();
        var count = ids.Count;

        return await _releaseRepository.GetAllAsync(q => q.Where(x =>
            (!x.ReleaseProjects.Any() || x.ReleaseProjects.Count(p => ids.Contains(p.ProjectId)) == count) &&
            !x.Deleted && x.Status == ReleaseStatus.Planning.ToInt()));
    }

    public async Task<IList<Release>> GetAllForAnyProjectAsync(IList<int> projectIds)
    {
        var ids = (projectIds ?? []).Distinct().ToList();

        return await _releaseRepository.GetAllAsync(q => q.Where(x => !x.Deleted &&
            (!x.ReleaseProjects.Any() || x.ReleaseProjects.Any(p => ids.Contains(p.ProjectId)))));
    }

    public async Task<bool> IsAvailableForAllProjectsAsync(int releaseId, IList<int> projectIds)
    {
        var mapped = (await GetProjectsAsync(releaseId)).Select(x => x.ProjectId).ToHashSet();
        return mapped.Count == 0 || (projectIds ?? []).All(mapped.Contains);
    }

    public async Task<IList<ReleaseProject>> GetProjectsAsync(int releaseId)
    {
        return await _releaseProjectRepository.GetAllAsync(q => q.Where(x => x.ReleaseId == releaseId));
    }

    public async Task SaveProjectsAsync(int releaseId, IList<int> projectIds)
    {
        var target = (projectIds ?? []).Where(x => x > 0).Distinct().ToHashSet();
        var existing = await GetProjectsAsync(releaseId);

        var toRemove = existing.Where(x => !target.Contains(x.ProjectId)).ToList();
        if (toRemove.Count > 0)
            await _releaseProjectRepository.DeleteAsync(toRemove);

        var existingIds = existing.Select(x => x.ProjectId).ToHashSet();
        var toAdd = target.Where(x => !existingIds.Contains(x))
            .Select(x => new ReleaseProject { ReleaseId = releaseId, ProjectId = x })
            .ToList();
        if (toAdd.Count > 0)
            await _releaseProjectRepository.InsertAsync(toAdd);
    }

    public async Task<Release> GetByIdAsync(int id)
    {
        return id == 0 ? null : await _releaseRepository.GetByIdAsync(id);
    }

    public async Task<Release> GetByNameAsync(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
            return null;

        var query = from c in _releaseRepository.Table
                    orderby c.Id
                    where !c.Deleted && c.Name == version
                    select c;
        return await query.FirstOrDefaultAsync();
    }

    public async Task<bool> NameExistsInScopeAsync(string name, IList<int> projectIds, int excludeReleaseId = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        var trimmed = name.Trim();
        var ids = (projectIds ?? []).Where(x => x > 0).Distinct().ToList();

        var query = _releaseRepository.Table
            .Where(x => !x.Deleted && x.Id != excludeReleaseId && x.Name == trimmed);

        if (ids.Count > 0)
            query = query.Where(x => !x.ReleaseProjects.Any() || x.ReleaseProjects.Any(p => ids.Contains(p.ProjectId)));

        return await query.AnyAsync();
    }

    public async Task InsertAsync(Release entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _releaseRepository.InsertAsync(entity);
    }

    public async Task InsertAsync(IList<Release> entities)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        await _releaseRepository.InsertAsync(entities);
    }

    public async Task UpdateAsync(Release entity)
    {

        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _releaseRepository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(Release entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _releaseRepository.DeleteAsync(entity);
    }

    #endregion
}
