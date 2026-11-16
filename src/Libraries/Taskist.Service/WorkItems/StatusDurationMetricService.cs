using Taskist.Core.Domain.WorkItems;
using Taskist.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace Taskist.Service.WorkItems;

public class StatusDurationMetricService : IStatusDurationMetricService
{
    #region Fields

    protected readonly IRepository<StatusDurationMetric> _metricRepository;

    #endregion

    #region Ctor

    public StatusDurationMetricService(IRepository<StatusDurationMetric> metricRepository)
    {
        _metricRepository = metricRepository;
    }

    #endregion

    #region Methods

    public async Task<IList<StatusDurationMetric>> GetAllAsync(bool showDeleted = false)
    {
        return await _metricRepository.GetAllAsync(q => q
            .Include(x => x.FromStatus)
            .Include(x => x.ToStatus)
            .Include(x => x.Project)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name), includeDeleted: showDeleted);
    }

    public async Task<IList<StatusDurationMetric>> GetActiveForProjectAsync(int projectId, bool onlyShowOnTask = false)
    {
        return await _metricRepository.GetAllAsync(q => q
            .Include(x => x.FromStatus)
            .Include(x => x.ToStatus)
            .Where(x => x.Active
                && (x.ProjectId == null || x.ProjectId == projectId)
                && (!onlyShowOnTask || x.ShowOnTask))
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name));
    }

    public async Task<StatusDurationMetric> GetByIdAsync(int id)
    {
        return id == 0 ? null : await _metricRepository.GetByIdAsync(id);
    }

    public async Task InsertAsync(StatusDurationMetric entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _metricRepository.InsertAsync(entity);
    }

    public async Task UpdateAsync(StatusDurationMetric entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _metricRepository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(StatusDurationMetric entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _metricRepository.DeleteAsync(entity);
    }

    public async Task ReplaceAllAsync(IList<StatusDurationMetric> metrics)
    {
        var existing = await _metricRepository.GetAllAsync();
        var keepIds = metrics.Where(m => m.Id > 0).Select(m => m.Id).ToHashSet();

        foreach (var stale in existing.Where(x => !keepIds.Contains(x.Id)))
            await _metricRepository.DeleteAsync(stale);

        var order = 0;
        foreach (var m in metrics)
        {
            order++;
            var current = m.Id > 0 ? existing.FirstOrDefault(x => x.Id == m.Id) : null;
            if (current == null)
            {
                await _metricRepository.InsertAsync(new StatusDurationMetric
                {
                    Name = m.Name,
                    FromStatusId = m.FromStatusId,
                    ToStatusId = m.ToStatusId,
                    ProjectId = m.ProjectId,
                    ShowOnTask = m.ShowOnTask,
                    DisplayOrder = order,
                    Active = m.Active
                });
            }
            else
            {
                current.Name = m.Name;
                current.FromStatusId = m.FromStatusId;
                current.ToStatusId = m.ToStatusId;
                current.ProjectId = m.ProjectId;
                current.ShowOnTask = m.ShowOnTask;
                current.DisplayOrder = order;
                current.Active = m.Active;
                await _metricRepository.UpdateAsync(current);
            }
        }
    }

    #endregion
}
