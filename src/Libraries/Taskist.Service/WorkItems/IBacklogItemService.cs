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

	Task<Backlog> GetByIdAsync(int id);

	Task InsertAsync(Backlog entity, List<int> documentIds);

	Task UpdateAsync(Backlog entity);

	Task<string> UpdateAsync(int id, string property, string value);


	Task DeleteAsync(Backlog entity);

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
