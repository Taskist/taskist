using Taskist.Core.Domain.Masters;
using Taskist.Core.Domain.WorkItems;
using Taskist.Core.Dtos.WorkItems;
using Taskist.Core.Enums;

namespace Taskist.Service.WorkItems;

public interface IClosingRequirementService
{
    bool IsRequired(Status status, TaskTypeGroup group, ClosingItem item);

    Task<ClosingRequirementDto> EvaluateAsync(Backlog task, Status status, int? taskTypeId = null, bool loadMergeRequests = false);

    Task<ClosingRequirementDto> EvaluateAsync(int backlogId, int statusId, int? taskTypeId = null, bool loadMergeRequests = false);
}
