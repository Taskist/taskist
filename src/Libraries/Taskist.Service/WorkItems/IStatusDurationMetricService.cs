using Taskist.Core.Domain.WorkItems;

namespace Taskist.Service.WorkItems;

public interface IStatusDurationMetricService
{
    Task<IList<StatusDurationMetric>> GetAllAsync(bool showDeleted = false);

    Task<IList<StatusDurationMetric>> GetActiveForProjectAsync(int projectId, bool onlyShowOnTask = false);

    Task<StatusDurationMetric> GetByIdAsync(int id);

    Task InsertAsync(StatusDurationMetric entity);

    Task UpdateAsync(StatusDurationMetric entity);

    Task DeleteAsync(StatusDurationMetric entity);

    Task ReplaceAllAsync(IList<StatusDurationMetric> metrics);
}
