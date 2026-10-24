using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Core.Domain.Users;
using Taskist.Core.Domain.WorkItems;
using Taskist.Core.Dtos.Charts;
using Taskist.Core.Dtos.Git;
using Taskist.Core.Dtos.Reports;
using Taskist.Core.Dtos.WorkItems;
using Taskist.Core.Enums;

namespace Taskist.Service.WorkItems;

public interface IBacklogService
{
    #region Methods

    Task<IPagedList<Backlog>> GetPagedListAsync(int assigneeId,
        int projectId,
        int[] createdby,
        int[] module,
        int[] subModule,
        int[] taskType,
        int[] severity,
        int[] reporter,
        int[] assignee,
        int[] tester,
        int[] status,
        int[] statusGroup,
        int[] release,
        int? dueDateType,
        DateOnly? dueDate,
        int[] accessiableProjects,
        string? search = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue,
        string sortColumn = "",
        string sortDirection = "",
        bool canViewOthersTask = false,
        TaskSource? source = null,
        int[] project = null,
        int[] viewAllProjects = null,
        int filterMode = 0,
        bool rootsOnly = false,
        int? statusDaysMin = null,
        int? openDaysMin = null);

    Task<IList<Backlog>> GetOpenAssignmentsAsync();

    Task<IList<ProjectOpenStatusSummaryDto>> GetOpenStatusSummaryAsync();

    Task<string> UpdateFieldsAsync(int id, IList<FieldChange> changes);

    Task<IPagedList<Backlog>> GetPagedListForPivotAsync(int userId,
        int? lastAccessedProjectId,
        string? projectName,
        string? moduleName,
        string? subModuleName,
        string? severityName,
        string? releaseName,
        string? reporterName,
        string? developerName,
        string? testerName,
        string? taskTypeName,
        string? statusName,
        string? search = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue,
        string sortColumn = "",
        string sortDirection = "");

    Task<IList<Backlog>> GetAllForExcelExportAsync(int assigneeId,
        int projectId,
        int[] createdby,
        int[] module,
        int[] subModule,
        int[] taskType,
        int[] severity,
        int[] reporter,
        int[] assignee,
        int[] tester,
        int[] status,
        int[] statusGroup,
        int[] release,
        int? dueDateType,
        DateOnly? dueDate,
        int[] accessiableProjects,
        string[] groupByColumns,
        bool canViewOthersTask = false,
        TaskSource? source = null,
        int[] project = null,
        int[] viewAllProjects = null,
        int? statusDaysMin = null,
        int? openDaysMin = null);

    Task<IList<Backlog>> GetAllAsync(int projectId);

    Task<IPagedList<Backlog>> GetBoardCardsAsync(int projectId, int statusId, int pageIndex, int pageSize, int userId, bool canViewOthers);

    Task<IList<Backlog>> GetTimelineTasksAsync(int projectId, DateOnly from, DateOnly to, int userId, bool canViewOthers);

    Task<IList<Backlog>> GetAllByReleaseAsync(int releaseId);

    Task<IList<Backlog>> GetReleaseProjectMismatchesAsync(int[] projectIds);

    Task<IList<ReleaseProjectProgressDto>> GetReleaseProjectProgressAsync(int[] releaseIds, int userId, int[] accessibleProjects, int[] viewAllProjects);

    Task<IList<Backlog>> GetBySourceWikiDocumentAsync(int wikiDocumentId);

    Task<Backlog> GetByIdAsync(int id);

    Task<IList<Backlog>> SearchByTitleAsync(string search, int excludeTaskId = 0);

    Task<IList<Backlog>> SearchSimilarInProjectAsync(int projectId, string title, int take = 5);

    Task<IList<Backlog>> SearchSimilarAsync(int[] projectIds, string title, int take = 8);

    Task<IList<Backlog>> GetChildrenAsync(int parentId, int userId, int[] accessibleProjects, int[] viewAllProjects);

    Task<Dictionary<int, int>> GetChildCountsAsync(int[] parentIds);

    Task<Dictionary<int, (DateTime First, DateTime Last)>> GetStatusChangeBoundsAsync(int[] taskIds);

    Task InsertAsync(Backlog entity, List<int> documentIds);

    Task UpdateAsync(Backlog entity);

    Task<string> UpdateAsync(int id, string property, string value);

    Task<List<int>> GetProjectIdsByTaskIdsAsync(int[] ids);

    Task<bool> CanEditTaskAsync(int id);

    Task<bool> CanViewTaskAsync(int id);

    Task<bool> CanDeleteTaskAsync(int id);

    Task UpdateBatchAsync(string ids, int? taskTypeId, int? reporterId, int? severityId,
       DateOnly? dueDate, int? moduleId, int? subModuleId, int? releaseId, int? assigneeId, int? testerId, int? statusId);

    Task UpdateStatusOnMergeActionAsync(GitMergeRequestInfo mergeRequest, User? actor, string? actorUserName, IList<int> taskIds, int? repositoryId, string mergeRequestLabel);

    Task<int> MoveTasksForOpenedMergeRequestAsync(IList<int> taskIds, int mergeRequestIid, User? actor, string? actorGitUserName, int? repositoryId, string mergeRequestLabel);

    Task DeleteAsync(Backlog entity);

    #endregion

    #region Documents

    Task<IList<BacklogDocument>> GetAllDocumentAsync(int backlogId);

    Task<BacklogDocument> GetAttachmentAsync(int id);

    Task InsertDocumentAsync(int backlogId, int documentId, int loggedUserId);

    Task<bool> DeleteDocumentAsync(int documentId, int loggedUserId);

    #endregion

    #region History

    Task<IList<BacklogComment>> GetAllHistoryAsync(int backlogId);

    #endregion

    #region Comments

    Task<IList<BacklogComment>> GetAllCommentsAsync(int backlogId);

    Task InsertCommentAsync(int backlogId, string comment, int loggedUserId, string loggedUserName, bool postToIssue = false);

    #endregion

    #region Status History

    Task<IList<BacklogStatusLog>> GetStatusLogByTaskId(int taskId);

    #endregion

    #region Reports

    Task<IDictionary<string, int>> GetTaskCountAsync(int userId, int[] projectIds = null);

    #endregion

    #region Charts

    Task<List<ReleaseVsTaskDto>> GetReleaseTaskDataAsync(int assigneeId, int[] projectIds = null);

    Task<Dictionary<string, int>> GetDueTasksAsync(int assigneeId, int[] projectIds = null);

    Task<List<TaskCountDto>> GetProjectTasksAsync(int assigneeId, int[] projectIds = null);

    #endregion

    #region Custom Fields

    Task<string> InsertFieldValueAsync(int backlogId, int customFieldId, string value);

    Task<IList<BacklogCustomFieldValue>> GetAllCustomFieldValuesAsync(int backlogId);


    Task<BacklogCustomFieldValue> GetCustomFieldValueAsync(int backlogId, int customFieldId);

    Task InsertFieldValueAsync(BacklogCustomFieldValue entity);

    Task InsertFieldValueAsync(IList<BacklogCustomFieldValue> entity);

    #endregion

    Task<List<BacklogPivotDto>> GetPivotDataAsync(int projectId);
}
