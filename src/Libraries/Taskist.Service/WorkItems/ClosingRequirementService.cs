using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Core.Domain.WorkItems;
using Taskist.Core.Dtos.Git;
using Taskist.Core.Dtos.WorkItems;
using Taskist.Core.Enums;
using Taskist.Data.Repository;
using Taskist.Service.Masters;

namespace Taskist.Service.WorkItems;

public class ClosingRequirementService : IClosingRequirementService
{
    #region Fields

    private readonly IRepository<Backlog> _backlogRepository;
    private readonly IStatusService _statusService;
    private readonly ITaskTypeService _taskTypeService;
    private readonly ITaskCodeService _taskCodeService;

    #endregion

    #region Ctor

    public ClosingRequirementService(IRepository<Backlog> backlogRepository,
        IStatusService statusService,
        ITaskTypeService taskTypeService,
        ITaskCodeService taskCodeService)
    {
        _backlogRepository = backlogRepository;
        _statusService = statusService;
        _taskTypeService = taskTypeService;
        _taskCodeService = taskCodeService;
    }

    #endregion

    #region Methods

    public bool IsRequired(Status status, TaskTypeGroup group, ClosingItem item)
    {
        if (status == null)
            return false;

        return (group, item) switch
        {
            (TaskTypeGroup.Bug, ClosingItem.RootCause) => status.RequiredRootCauseForBug,
            (TaskTypeGroup.Bug, ClosingItem.Resolution) => status.RequiredResolutionForBug,
            (TaskTypeGroup.Bug, ClosingItem.MergeRequest) => status.RequiredMrNumberForBug,
            (TaskTypeGroup.CR, ClosingItem.RootCause) => status.RequiredRootCauseForCr,
            (TaskTypeGroup.CR, ClosingItem.Resolution) => status.RequiredResolutionForCr,
            (TaskTypeGroup.CR, ClosingItem.MergeRequest) => status.RequiredMrNumberForCr,
            _ => false
        };
    }

    public async Task<ClosingRequirementDto> EvaluateAsync(int backlogId, int statusId, int? taskTypeId = null, bool loadMergeRequests = false)
    {
        var task = await _backlogRepository.GetByIdAsync(backlogId);
        var status = await _statusService.GetByIdAsync(statusId);

        if (task == null || status == null)
            return new ClosingRequirementDto();

        return await EvaluateAsync(task, status, taskTypeId, loadMergeRequests);
    }

    public async Task<ClosingRequirementDto> EvaluateAsync(Backlog task, Status status, int? taskTypeId = null, bool loadMergeRequests = false)
    {
        var result = new ClosingRequirementDto
        {
            StatusName = status?.Name,
            HasRootCause = HtmlText.HasContent(task.RootCauseAnalysis),
            HasResolution = HtmlText.HasContent(task.ResolutionDetails),
            ClosingMergeRequestId = task.MergeRequestId > 0 ? task.MergeRequestId : null
        };

        var taskType = taskTypeId > 0 && taskTypeId != task.TaskTypeId
            ? await _taskTypeService.GetByIdAsync(taskTypeId.Value)
            : task.TaskType;

        if (taskType == null || status == null)
            return result;

        var group = (TaskTypeGroup)taskType.GroupId;
        result.TaskTypeGroup = taskType.GroupId;
        result.RootCauseRequired = IsRequired(status, group, ClosingItem.RootCause);
        result.ResolutionRequired = IsRequired(status, group, ClosingItem.Resolution);
        result.MrRequired = IsRequired(status, group, ClosingItem.MergeRequest);

        result.HasMr = result.ClosingMergeRequestId != null;

        if (loadMergeRequests && result.MrRequired && !result.HasMr)
        {
            var code = await _taskCodeService.GetTaskCodeAsync(task.Id);

            if (code.State == TaskCodeState.Ok)
            {
                result.LinkedMergeRequests = code.MergeRequests
                    .Where(m => !string.Equals(m.State, "closed", StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            else if (code.State == TaskCodeState.Unreachable)
            {
                result.MrLookupFailed = true;
                result.MrLookupMessage = code.ErrorMessage;
            }
        }

        return result;
    }

    #endregion
}
