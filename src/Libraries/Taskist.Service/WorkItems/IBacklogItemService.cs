using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Core.Domain.WorkItems;

namespace Taskist.Service.WorkItems;

public interface IBacklogItemService
{
	Task<IPagedList<Backlog>> GetPagedListAsync(int assigneeId,
		int projectId,
		int[] createdby,
		int[] module,
		int[] subModule,
		int[] taskType,
		int[] severity,
		int[] reporter,
		int[] assignee,
		int[] status,
		int[] sprint,
		int[] accessiableProjects,
		int pageIndex = 0,
		int pageSize = int.MaxValue,
		string sortColumn = "",
		string sortDirection = "",
		bool canViewOthersTask = false);

	Task<IList<Backlog>> GetAllForExcelExportAsync(int assigneeId,
		int projectId,
		int[] createdby,
		int[] module,
		int[] subModule,
		int[] taskType,
		int[] severity,
		int[] reporter,
		int[] assignee,
		int[] status,
		int[] sprint,
		int[] accessiableProjects,
		string[] groupByColumns,
		bool canViewOthersTask = false);

	Task<IList<Backlog>> GetAllAsync(int projectId);

	/// <summary>All non-deleted tasks across the user's accessible projects (dashboard).</summary>
	Task<IList<Backlog>> GetAccessibleTasksAsync(int userId);

	/// <summary>Most recent comments/activity across the user's accessible projects.</summary>
	Task<IList<BacklogComment>> GetRecentActivityAsync(int userId, int take);

	Task<Backlog> GetByIdAsync(int id);

	Task InsertAsync(Backlog entity, List<int> documentIds);

	Task UpdateAsync(Backlog entity);

	/// <summary>
	/// Builds the task's status journey: each status it passed through and how long it was held.
	/// </summary>
	Task<IList<(string StatusName, string TextColor, string BackgroundColor, string IconClass, int Days, bool IsCurrent)>> GetStatusJourneyAsync(int backlogId);

	/// <summary>
	/// Writes a system-comment history entry for each changed field (key = field label,
	/// value = old/new display values). Used by the full-form Edit save.
	/// </summary>
	Task LogEditHistoryAsync(int backlogId, IDictionary<string, (string Old, string New)> changes, int userId);

	/// <summary>
	/// Records a status-log row and sends the resolved/reopened notification email.
	/// </summary>
	Task LogStatusChangeAsync(Backlog entity, int newStatusId, int userId, string oldStatusName, string newStatusName);

	Task<string> UpdateAsync(int id, string property, string value);


	Task DeleteAsync(Backlog entity);
	Task DeleteBacklogAsync(int id);

	/// <summary>
	/// Indicates whether the user is a member of the project the item belongs to.
	/// </summary>
	Task<bool> CanAccessAsync(int backlogId, int userId);

	/// <summary>
	/// Indicates whether the user may reach the item a document is attached to.
	/// </summary>
	Task<bool> CanAccessDocumentAsync(int backlogDocumentId, int userId);

    #region Documents

    Task<IList<BacklogDocument>> GetAllDocumentAsync(int backlogId);

	Task<Document> GetDocumentByIdAsync(int id);

	Task InsertDocumentAsync(int backlogId, int documentId, int loggedUserId);

	Task<bool> DeleteDocumentAsync(int documentId, int loggedUserId);

	#endregion

	#region History

	Task<IList<BacklogComment>> GetAllHistoryAsync(int backlogId);

	#endregion

	#region Comments

	Task<IList<BacklogComment>> GetAllCommentsAsync(int backlogId);

	Task InsertCommentAsync(int backlogId, string comment, int loggedUserId);

	#endregion

	#region Reports

	Task<IDictionary<string, int>> GetTaskCountAsync(int userId);

	#endregion

	#region Custom Fields

	Task<string> InsertFieldValueAsync(int backlogId, int customFieldId, string value);

	Task<IList<BacklogCustomFieldValue>> GetAllCustomFieldValuesAsync(int backlogId);

	Task<BacklogCustomFieldValue> GetCustomFieldValueAsync(int backlogId, int customFieldId);

	Task InsertFieldValueAsync(BacklogCustomFieldValue entity);

	Task InsertFieldValueAsync(IList<BacklogCustomFieldValue> entity);

	#endregion
}
