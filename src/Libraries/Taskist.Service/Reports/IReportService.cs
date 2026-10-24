using Taskist.Core.Dtos.Reports;

namespace Taskist.Service.Reports;

public interface IReportService
{
    Task<List<TrendPointDto>> GetCreatedVsResolvedAsync(int[] projectIds, int months = 6);

    Task<List<WorkloadDto>> GetAssigneeWorkloadAsync(int[] projectIds);

    Task<ResourceLoadResultDto> GetResourceLoadAsync(ResourceLoadFilter filter, int currentUserId, int[] accessibleProjectIds);

    Task<List<TimeInStatusDto>> GetTimeInStatusAsync(int[] projectIds);

    Task<List<CycleTimeDto>> GetCycleTimeAsync(int[] projectIds);

    Task<List<StageDurationDto>> GetStageDurationAsync(int[] projectIds, DateTime? from = null, DateTime? to = null, int? taskTypeId = null);

    Task<List<StageDurationTaskDto>> GetStageDurationTasksAsync(int metricId, int[] projectIds, DateTime? from = null, DateTime? to = null, int? taskTypeId = null);
}
