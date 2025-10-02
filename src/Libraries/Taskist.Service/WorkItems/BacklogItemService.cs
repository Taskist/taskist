using LinqToDB;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text.RegularExpressions;
using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Core.Domain.WorkItems;
using Taskist.Core.Extensions;
using Taskist.Data.Repository;
using Taskist.Service.Masters;
using Taskist.Service.Messages;
using Taskist.Service.Users;

namespace Taskist.Service.WorkItems;

public class BacklogItemService : IBacklogItemService
{
    #region Fields

    protected readonly IRepository<Backlog> _backlogRepository;
    protected readonly IRepository<BacklogComment> _backlogCommentRepository;
    protected readonly IRepository<BacklogDocument> _backlogDocumentRepository;
    protected readonly IRepository<BacklogStatusLog> _backlogStatusLogRepository;
    protected readonly IRepository<BacklogCustomFieldValue> _backlogCustomFieldValueRepository;
    protected readonly IUserService _userService;
    protected readonly ICustomFieldService _customFieldService;
    protected readonly IMessageService _messageService;
    protected readonly IWorkContext _workContext;

    #endregion

    #region Ctor
    public BacklogItemService(IRepository<Backlog> backlogRepository,
        IRepository<BacklogDocument> backlogDocumentRepository,
        IRepository<BacklogComment> backlogCommentRepository,
        IRepository<BacklogStatusLog> backlogStatusLogRepository,
        IRepository<BacklogCustomFieldValue> backlogCustomFieldValueRepository,
        IUserService userService,
        ICustomFieldService customFieldService,
        IMessageService messageService,
        IWorkContext workContext)
    {
        _backlogRepository = backlogRepository;
        _backlogDocumentRepository = backlogDocumentRepository;
        _backlogCommentRepository = backlogCommentRepository;
        _backlogStatusLogRepository = backlogStatusLogRepository;
        _backlogCustomFieldValueRepository = backlogCustomFieldValueRepository;
        _userService = userService;
        _customFieldService = customFieldService;
        _messageService = messageService;
        _workContext = workContext;

    }
    #endregion

    #region Methods

    public async Task<IPagedList<Backlog>> GetPagedListAsync(int assigneeId,
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
        bool canViewOthersTask = false)
    {
        return await _backlogRepository.GetAllPagedAsync(query =>
        {
            if (projectId > 0)
                query = query.Where(x => x.ProjectId == projectId);
            else
                query = query.Where(x => accessiableProjects.Contains(x.ProjectId));

            if (!canViewOthersTask)
                query = query.Where(x => x.AssigneeId == assigneeId);

            query = ApplyFilters(query,
                    createdby,
                    module,
                    subModule,
                    taskType,
                    severity,
                    reporter,
                    assignee,
                    status,
                    sprint);

            query = query.OrderBy($"{sortColumn} {sortDirection}");

            return query;
        }, pageIndex, pageSize);
    }

    public async Task<IList<Backlog>> GetAllForExcelExportAsync(int assigneeId,
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
        bool canViewOthersTask = false)
    {
        return await _backlogRepository.GetAllAsync(query =>
        {
            if (projectId > 0)
                query = query.Where(x => x.ProjectId == projectId);
            else
                query = query.Where(x => accessiableProjects.Contains(x.ProjectId));

            if (!canViewOthersTask)
                query = query.Where(x => x.AssigneeId == assigneeId);

            query = ApplyFilters(query,
                    createdby,
                    module,
                    subModule,
                    taskType,
                    severity,
                    reporter,
                    assignee,
                    status,
                    sprint);

            if (groupByColumns.Any())
                query = query.OrderBy(string.Join(",", groupByColumns));

            return query;
        }, false);
    }

    public async Task<IList<Backlog>> GetAllAsync(int projectId)
    {
        return await _backlogRepository.GetAllAsync(query =>
        {
            if (projectId > 0)
                query = query.Where(x => x.ProjectId == projectId);

            return query;
        }, false);
    }

    public async Task<Backlog> GetByIdAsync(int id)
    {
        return id == 0 ? null : await _backlogRepository.GetByIdAsync(id);
    }

    public async Task InsertAsync(Backlog entity, List<int> documentIds)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _backlogRepository.InsertAsync(entity);
        if (entity.Id > 0 && documentIds.Count > 0)
        {
            var documentEntities = new List<BacklogDocument>();
            foreach (var id in documentIds)
            {
                documentEntities.Add(new BacklogDocument
                {
                    BacklogId = entity.Id,
                    DocumentId = id,
                    CreatedById = entity.CreatedById,
                    CreatedOn = entity.CreatedOn
                });
            }

            await _backlogDocumentRepository.InsertAsync(documentEntities);

            await _backlogCommentRepository.InsertAsync(new BacklogComment
            {
                BacklogId = entity.Id,
                CreatedById = entity.CreatedById,
                CreatedOn = DateTime.UtcNow,
                Comment = $"New {entity.TaskType.Name} created.",
                SystemComment = true
            });

            await _backlogStatusLogRepository.InsertAsync(new BacklogStatusLog
            {
                BacklogId = entity.Id,
                StatusId = entity.StatusId,
                CreatedById = entity.CreatedById,
                CreatedOn = DateTime.Now
            });
        }
    }

    public async Task UpdateAsync(Backlog entity)
    {

        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _backlogRepository.UpdateAsync(entity);
    }

    public async Task<string> UpdateAsync(int id, string property, string value)
    {
        string message = string.Empty;
        try
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var oldValue = CommonHelper.GetPropertyValue(entity, CleanPropertyName(property));

            var updatedEntity = UpdateEntityProperty(entity, property, value);

            await _backlogRepository.UpdateAsync(entity);

            var newValue = CommonHelper.GetPropertyValue(entity, CleanPropertyName(property));

            var loggedUser = await _workContext.GetCurrentUserAsync();

            message = $"{CommonHelper.Beautify(property)} updated!";

            await _backlogCommentRepository.InsertAsync(new BacklogComment
            {
                BacklogId = id,
                CreatedById = loggedUser.Id,
                CreatedOn = DateTime.UtcNow,
                Comment = $"{CommonHelper.Beautify(property)}: {oldValue} → {newValue}",
                SystemComment = true
            });

            if (property == nameof(Backlog.StatusId))
            {
                await _backlogStatusLogRepository.InsertAsync(new BacklogStatusLog
                {
                    BacklogId = id,
                    StatusId = value.ToInt(),
                    CreatedById = loggedUser.Id,
                    CreatedOn = DateTime.Now
                });

                if (value.ToInt() == StatusGroupEnum.Resolved.ToInt() ||
                    value.ToInt() == StatusGroupEnum.ReOpened.ToInt())
                {
                    await _messageService.EmailTaskNotificationAsync(entity, loggedUser, $"{oldValue} → {newValue}");
                }
            }

            if (property == nameof(Backlog.AssigneeId) && entity.Assignee != null &&
                entity.AssigneeId > 0)
            {
                await _messageService.EmailNewTaskNotificationAsync(entity);
            }
        }
        catch (Exception ex)
        {
            message = $"Error: {ex.Message}";
        }

        return message;
    }

    private string CleanPropertyName(string property)
    {
        return Regex.Replace(property, "Id$", ".Name", RegexOptions.IgnoreCase);
    }

    public async Task DeleteAsync(Backlog entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _backlogRepository.DeleteAsync(entity);
    }

    #endregion

    #region Methods For Documents

    public async Task<IList<BacklogDocument>> GetAllDocumentAsync(int backlogId)
    {
        return await _backlogDocumentRepository.GetAllAsync(query =>
        {
            query = query.Where(x => x.BacklogId == backlogId);
            return query;
        }, false);
    }

    public async Task<Document> GetDocumentByIdAsync(int id)
    {
        return await _backlogDocumentRepository.Table.Where(x => x.Id == id)
            .Select(x => x.Document)
            .FirstOrDefaultAsync();
    }

    public async Task InsertDocumentAsync(int backlogId, int documentId, int loggedUserId)
    {
        var entity = await GetByIdAsync(backlogId);

        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        if (entity.Id > 0 && documentId > 0)
        {
            var docEntity = new BacklogDocument
            {
                BacklogId = entity.Id,
                DocumentId = documentId,
                CreatedById = loggedUserId,
                CreatedOn = DateTime.Now
            };
            await _backlogDocumentRepository.InsertAsync(docEntity);

            if (docEntity.Id > 0)
            {
                await _backlogCommentRepository.InsertAsync(new BacklogComment
                {
                    BacklogId = backlogId,
                    CreatedById = loggedUserId,
                    CreatedOn = DateTime.UtcNow,
                    Comment = $"Added file {docEntity.Document.FileName}",
                    SystemComment = true
                });
            }
        }
    }

    public async Task<bool> DeleteDocumentAsync(int documentId, int loggedUserId)
    {
        var entity = await _backlogDocumentRepository.GetByIdAsync(documentId);

        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var commentEntity = new BacklogComment
        {
            BacklogId = entity.BacklogId,
            CreatedById = loggedUserId,
            CreatedOn = DateTime.UtcNow,
            Comment = $"Deleted file {entity.Document.FileName}",
            SystemComment = true
        };

        await _backlogDocumentRepository.DeleteAsync(entity);

        var entityExists = await _backlogDocumentRepository.GetByIdAsync(documentId);
        if (entityExists == null)
        {
            await _backlogCommentRepository.InsertAsync(commentEntity);
            return true;
        }

        return false;
    }

    #endregion

    #region Methods For History

    public async Task<IList<BacklogComment>> GetAllHistoryAsync(int backlogId)
    {
        return await _backlogCommentRepository.GetAllAsync(query =>
        {
            query = query.Where(x => x.BacklogId == backlogId && x.SystemComment).OrderByDescending(x => x.CreatedOn);
            return query;
        }, false);
    }

    #endregion

    #region Methods For Comments

    public async Task<IList<BacklogComment>> GetAllCommentsAsync(int backlogId)
    {
        return await _backlogCommentRepository.GetAllAsync(query =>
        {
            query = query.Where(x => x.BacklogId == backlogId && !x.SystemComment).OrderByDescending(x => x.CreatedOn);
            return query;
        }, false);
    }

    public async Task InsertCommentAsync(int backlogId, string comment, int loggedUserId)
    {
        var entity = await GetByIdAsync(backlogId);

        if (backlogId <= 0)
            throw new ArgumentNullException(nameof(backlogId));

        await _backlogCommentRepository.InsertAsync(new BacklogComment
        {
            BacklogId = backlogId,
            Comment = comment,
            CreatedById = loggedUserId,
            CreatedOn = DateTime.Now,
            SystemComment = false
        });

        await _backlogCommentRepository.InsertAsync(new BacklogComment
        {
            BacklogId = backlogId,
            CreatedById = loggedUserId,
            CreatedOn = DateTime.UtcNow,
            Comment = $"Added a comment",
            SystemComment = true
        });
    }

    #endregion

    #region Methods For Reports

    public async Task<IDictionary<string, int>> GetTaskCountAsync(int userId)
    {
        var data = new Dictionary<string, int>();

        var projects = await _userService.GetAllAccessibleProjects(userId);
        var projectsIds = projects.Select(x => x.Id);
        var totalTasks = await _backlogRepository.Table.Where(x => projectsIds.Contains(x.ProjectId)).ToListAsync();
        var myTasksCount = totalTasks.Where(x => x.AssigneeId == userId).Count();
        var closedTasksCount = totalTasks.Where(x => x.AssigneeId == userId &&
            x.Status.GroupId == (int)StatusGroupEnum.Resolved).Count();

        data.Add("Total Tasks", totalTasks.Count);
        data.Add("My Tasks", myTasksCount);
        data.Add("Closed Tasks", closedTasksCount);

        return data;
    }

    #endregion

    #region Methods For Custom Fields

    public async Task<IList<BacklogCustomFieldValue>> GetAllCustomFieldValuesAsync(int backlogId)
    {
        return await _backlogCustomFieldValueRepository.GetAllAsync(query =>
        {
            query = query.Where(x => x.BacklogId == backlogId);
            return query;
        }, false);
    }

    public async Task<BacklogCustomFieldValue> GetCustomFieldValueAsync(int backlogId, int customFieldId)
    {
        return await _backlogCustomFieldValueRepository.Table
            .Where(x => x.BacklogId == backlogId && x.CustomFieldId == customFieldId)
            .FirstOrDefaultAsync();
    }

    public async Task<string> InsertFieldValueAsync(int backlogId, int customFieldId, string value)
    {
        string message = string.Empty;

        try
        {
            if (backlogId <= 0 || customFieldId <= 0)
                throw new ArgumentNullException(nameof(backlogId));

            var existence = await _backlogCustomFieldValueRepository.Table
                .FirstOrDefaultAsync(x => x.BacklogId == backlogId && x.CustomFieldId == customFieldId);

            var customField = await _customFieldService.GetByIdAsync(customFieldId);
            var oldValue = string.Empty;

            if (existence == null)
            {
                await _backlogCustomFieldValueRepository.InsertAsync(new BacklogCustomFieldValue
                {
                    BacklogId = backlogId,
                    CustomFieldId = customFieldId,
                    Value = value
                });
            }
            else
            {
                oldValue = existence.Value;
                existence.Value = value;

                await _backlogCustomFieldValueRepository.UpdateAsync(existence);

                message = $"{CommonHelper.Beautify(customField.Label)} updated!";
            }


            var loggedUser = await _workContext.GetCurrentUserAsync();
            await _backlogCommentRepository.InsertAsync(new BacklogComment
            {
                BacklogId = backlogId,
                CreatedById = loggedUser.Id,
                CreatedOn = DateTime.UtcNow,
                Comment = $"{CommonHelper.Beautify(customField.Label)}: {oldValue} → {value}",
                SystemComment = true
            });
        }
        catch (Exception ex)
        {
            message = "Error";
        }

        return message;
    }

    public async Task InsertFieldValueAsync(BacklogCustomFieldValue entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _backlogCustomFieldValueRepository.InsertAsync(entity);
    }

    public async Task InsertFieldValueAsync(IList<BacklogCustomFieldValue> entities)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        await _backlogCustomFieldValueRepository.InsertAsync(entities);
    }

    #endregion

    #region Helpers

    private IQueryable<Backlog> ApplyFilters(IQueryable<Backlog> query,
        int[] createdby,
        int[] module,
        int[] subModule,
        int[] taskType,
        int[] severity,
        int[] reporter,
        int[] assignee,
        int[] status,
        int[] sprint)
    {
        if (createdby.Any())
        {
            query = query.Where(x => createdby.Contains(x.CreatedById));
        }

        if (module.Any())
        {
            query = query.Where(x => module.Contains(x.ModuleId ?? 0));
        }

        if (subModule.Any())
        {
            query = query.Where(x => subModule.Contains(x.SubModuleId ?? 0));
        }

        if (taskType.Any())
        {
            query = query.Where(x => taskType.Contains(x.TaskTypeId));
        }

        if (severity.Any())
        {
            query = query.Where(x => severity.Contains(x.SeverityId));
        }

        if (reporter.Any())
        {
            query = query.Where(x => reporter.Contains(x.ReporterId ?? 0));
        }

        if (assignee.Any())
        {
            query = query.Where(x => assignee.Contains(x.AssigneeId ?? 0));
        }

        if (status.Any())
        {
            query = query.Where(x => status.Contains(x.StatusId));
        }

        if (sprint.Any())
        {
            query = query.Where(x => sprint.Contains(x.SprintId ?? 0));
        }

        return query;
    }

    public TEntity UpdateEntityProperty<TEntity>(TEntity entity, string propertyName, object newValue) where TEntity : class
    {
        var entityType = typeof(TEntity);

        var property = entityType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (property != null)
        {
            var propertyType = property.PropertyType;
            if (newValue != null && newValue.ToString() == "-1")
                newValue = null;

            object convertedValue;

            if (propertyType == typeof(DateOnly) ||
            (propertyType.IsGenericType &&
             propertyType.GetGenericTypeDefinition() == typeof(Nullable<>) &&
             propertyType.GetGenericArguments()[0] == typeof(DateOnly)) && newValue != null)
            {
                convertedValue = DateOnly.FromDateTime(Convert.ToDateTime(newValue));
            }
            else
            {
                var targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
                convertedValue = newValue == null ? null : Convert.ChangeType(newValue, targetType);
            }

            property.SetValue(entity, convertedValue);

            return entity;
        }

        throw new ArgumentException($"Property '{propertyName}' not found on type '{entityType.Name}'.");
    }

    #endregion
}
