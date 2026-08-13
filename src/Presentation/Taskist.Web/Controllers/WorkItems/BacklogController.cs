using System.Net.Mime;
using OfficeOpenXml;
using LicenseContext = OfficeOpenXml.LicenseContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using AutoMapper;
using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Core.Domain.WorkItems;
using Taskist.Core.Extensions;
using Taskist.Web.Helpers.Extensions;
using Taskist.Service.Security;
using Taskist.Service.Masters;
using Taskist.Service.WorkItems;
using Taskist.Service.Localization;
using Taskist.Service.Logging;
using Taskist.Service.Users;
using Taskist.Service.Messages;
using Taskist.Web.Controllers.Common;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Datatable;
using Taskist.Web.Models.Masters;
using Taskist.Web.Models.WorkItems;

namespace Taskist.Web.Controllers.WorkItems;

public class BacklogController : BaseController
{
    #region Fields

    protected readonly IBacklogItemService _backlogItemService;
    protected readonly IUserService _userService;
    protected readonly IProjectService _projectService;
    protected readonly ITaskTypeService _taskTypeService;
    protected readonly IReporterService _reporterService;
    protected readonly ISeverityService _severityService;
    protected readonly IModuleService _moduleService;
    protected readonly ISubModuleService _subModuleService;
    protected readonly IStatusService _statusService;
    protected readonly ISprintService _sprintService;
    protected readonly IDocumentService _documentService;
    protected readonly ICustomFieldService _customFieldService;
    protected readonly IMessageService _messageService;
    protected readonly IPermissionService _permissionService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IUserActivityService _userActivityService;
    protected readonly IGenericAttributeService _genericAttributeService;
    protected readonly IWorkContext _workContext;
    protected readonly IMapper _mapper;

    #endregion

    #region Ctor

    public BacklogController(IBacklogItemService backlogItemService,
        IUserService userService,
        IProjectService projectService,
        ITaskTypeService taskTypeService,
        IReporterService reporterService,
        ISeverityService severityService,
        IModuleService moduleService,
        ISubModuleService subModuleService,
        IStatusService statusService,
        ISprintService sprintService,
        IDocumentService documentService,
        ICustomFieldService customFieldService,
        IMessageService messageService,
        IPermissionService permissionService,
        ILocalizationService localizationService,
        IUserActivityService userActivityService,
        IGenericAttributeService genericAttributeService,
        IWorkContext workContext,
        IMapper mapper)
    {
        _backlogItemService = backlogItemService;
        _userService = userService;
        _projectService = projectService;
        _taskTypeService = taskTypeService;
        _reporterService = reporterService;
        _severityService = severityService;
        _moduleService = moduleService;
        _subModuleService = subModuleService;
        _statusService = statusService;
        _sprintService = sprintService;
        _documentService = documentService;
        _customFieldService = customFieldService;
        _messageService = messageService;
        _permissionService = permissionService;
        _localizationService = localizationService;
        _userActivityService = userActivityService;
        _genericAttributeService = genericAttributeService;
        _workContext = workContext;
        _mapper = mapper;
    }

    #endregion

    #region Actions

    [CheckPermission(PermissionProvider.WorkItem.MANAGE_BACKLOGLOG)]
    public async Task<IActionResult> Index(int filter = 0)
    {
        var model = await GetProjectAccess();
        model.FilterMode = filter;

        return View(model);
    }

    [CheckPermission(PermissionProvider.WorkItem.MANAGE_BACKLOGLOG)]
    public async Task<IActionResult> Create()
    {
        var projectAccess = await GetProjectAccess();
        if (projectAccess == null || !projectAccess.CanReport && !projectAccess.CanEdit && !projectAccess.CanClose)
            return AccessDenied();

        var model = new BacklogModel();
        await InitModelAsync(model);

        return View(model);
    }

    [HttpPost, FormName("save-continue", "addNew")]
    [CheckPermission(PermissionProvider.WorkItem.MANAGE_BACKLOGLOG)]
    public async Task<IActionResult> Create(BacklogModel model, List<CustomFieldValueModel> fieldValues, bool addNew)
    {
        var loggedUser = await _workContext.GetCurrentUserAsync();
        var lastAccessedProjectId = await _genericAttributeService.GetAttributeAsync<int>(loggedUser, Constant.ActiveProjectSession);
        var fieldValuesErrors = false;

        if (fieldValues.Any())
        {
            var customFields = (await _customFieldService.GetAllMandatoryAsync(lastAccessedProjectId)).ToList();
            if (customFields.Any())
            {
                var emptyValues = customFields.Where(x => fieldValues.Any(y => y.CustomFieldId == x.Id && string.IsNullOrEmpty(y.Value)));
                fieldValuesErrors = emptyValues.Any();

                foreach (var customField in emptyValues)
                {
                    ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync($"{customField.ResourceKey}.RequiredMsg"));
                }
            }
        }

        if (ModelState.IsValid)
        {
            var entity = _mapper.Map<Backlog>(model);
            entity.ProjectId = lastAccessedProjectId;
            entity.CreatedById = loggedUser.Id;
            entity.CreatedOn = DateTime.UtcNow;

            var documentIds = new List<int>();
            if (!string.IsNullOrEmpty(model.Token))
                documentIds = JsonConvert.DeserializeObject<List<int>>(model.Token);

            await _backlogItemService.InsertAsync(entity, documentIds);

            if (entity.Id > 0)
            {
                #region Save Custom Field Values

                var entities = _mapper.Map<List<BacklogCustomFieldValue>>(fieldValues);
                if (entities.Any())
                {
                    entities.ForEach(f => f.BacklogId = entity.Id);
                    await _backlogItemService.InsertFieldValueAsync(entities);
                }

                #endregion

                if (entity.AssigneeId != null && entity.AssigneeId > 0)
                {
                    await _messageService.EmailNewTaskNotificationAsync(entity);
                }

                await _userActivityService.InsertAsync("BackLog", string.Format(await _localizationService.GetResourceAsync("Log.RecordCreated"), entity.Code), entity);
                return addNew ? RedirectToAction("Create") : RedirectToAction("Index");
            }

            await _userActivityService.InsertAsync("BackLog", string.Format(await _localizationService.GetResourceAsync("Log.RecordError"), entity.Code), entity);
            ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Error.UnableToCreateTask"));
        }

        if (!fieldValuesErrors)
            ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Error.UnableToCreateTask"));

        await InitModelAsync(model);

        return View(model);
    }

    [CheckPermission(PermissionProvider.WorkItem.MANAGE_BACKLOGLOG)]
    public async Task<IActionResult> Edit(int id)
    {
        var projectAccess = await GetProjectAccess();
        if (projectAccess == null || !projectAccess.CanReport && !projectAccess.CanEdit && !projectAccess.CanClose)
             return AccessDenied();

        var entity = await _backlogItemService.GetByIdAsync(id);
        if (entity == null)
            return RedirectToAction("Index");

        var model = _mapper.Map<BacklogModel>(entity);

        await InitModelAsync(model);
        model.CanEdit = await _permissionService.AuthorizeAsync(PermissionProvider.WorkItem.MANAGE_BACKLOGLOG);
        model.CanDelete = await _permissionService.AuthorizeAsync(PermissionProvider.WorkItem.MANAGE_BACKLOGLOG);

        //meta header + status journey ribbon
        model.CreatedOn = entity.CreatedOn;
        model.CreatedByName = entity.CreatedBy?.Name;
        var journey = await _backlogItemService.GetStatusJourneyAsync(id);
        model.StatusJourney = journey.Select(j => new BacklogStatusJourneyModel
        {
            StatusName = j.StatusName,
            TextColor = j.TextColor,
            BackgroundColor = j.BackgroundColor,
            IconClass = j.IconClass,
            Days = j.Days,
            IsCurrent = j.IsCurrent
        }).ToList();

        return View(model);
    }

    [HttpPost]
    [CheckPermission(PermissionProvider.WorkItem.MANAGE_BACKLOGLOG)]
    public async Task<IActionResult> Edit(BacklogModel model, List<CustomFieldValueModel> fieldValues)
    {
        var loggedUser = await _workContext.GetCurrentUserAsync();

        //the item must belong to a project the caller can reach
        if (!await _backlogItemService.CanAccessAsync(model.Id, loggedUser.Id))
            return AccessDenied();

        var entity = await _backlogItemService.GetByIdAsync(model.Id);
        if (entity == null)
            return RedirectToAction("Index");

        var fieldValuesErrors = false;

        if (fieldValues.Any())
        {
            var mandatoryFields = (await _customFieldService.GetAllMandatoryAsync(entity.ProjectId)).ToList();
            if (mandatoryFields.Any())
            {
                var emptyValues = mandatoryFields.Where(x => fieldValues.Any(y => y.CustomFieldId == x.Id && string.IsNullOrEmpty(y.Value)));
                fieldValuesErrors = emptyValues.Any();

                foreach (var customField in emptyValues)
                {
                    ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync($"{customField.ResourceKey}.RequiredMsg"));
                }
            }
        }

        if (ModelState.IsValid)
        {
            //capture the "before" state (resolved to display names) so we can log
            //a history entry per changed field once the save completes
            var before = new
            {
                entity.StatusId,
                StatusName = entity.Status?.Name,
                entity.AssigneeId,
                AssigneeName = entity.Assignee?.Name,
                entity.SeverityId,
                SeverityName = entity.Severity?.Name,
                entity.TaskTypeId,
                TaskTypeName = entity.TaskType?.Name,
                entity.ModuleId,
                ModuleName = entity.Module?.Name,
                entity.SubModuleId,
                SubModuleName = entity.SubModule?.Name,
                entity.SprintId,
                SprintName = entity.Sprint?.Name,
                entity.ReporterId,
                ReporterName = entity.Reporter?.Name,
                entity.DueDate,
                entity.Title
            };

            //preserve audit / ownership fields that the edit form does not post,
            //so mapping the model onto the entity cannot zero them out
            var createdById = entity.CreatedById;
            var createdOn = entity.CreatedOn;
            var code = entity.Code;

            _mapper.Map(model, entity);

            entity.CreatedById = createdById;
            entity.CreatedOn = createdOn;
            entity.Code = code;
            entity.ModifiedById = loggedUser.Id;
            entity.ModifiedOn = DateTime.UtcNow;

            await _backlogItemService.UpdateAsync(entity);

            //diff before/after and log a system-comment history entry per change
            var changes = new Dictionary<string, (string, string)>();
            void Track(string label, int? oldId, string oldName, int? newId, string newName)
            {
                if (oldId != newId)
                    changes[label] = (string.IsNullOrEmpty(oldName) ? "None" : oldName,
                                      string.IsNullOrEmpty(newName) ? "None" : newName);
            }

            Track("Status", before.StatusId, before.StatusName, entity.StatusId, entity.Status?.Name);
            Track("Assignee", before.AssigneeId, before.AssigneeName, entity.AssigneeId, entity.Assignee?.Name);
            Track("Severity", before.SeverityId, before.SeverityName, entity.SeverityId, entity.Severity?.Name);
            Track("Task Type", before.TaskTypeId, before.TaskTypeName, entity.TaskTypeId, entity.TaskType?.Name);
            Track("Module", before.ModuleId, before.ModuleName, entity.ModuleId, entity.Module?.Name);
            Track("Sub Module", before.SubModuleId, before.SubModuleName, entity.SubModuleId, entity.SubModule?.Name);
            Track("Sprint", before.SprintId, before.SprintName, entity.SprintId, entity.Sprint?.Name);
            Track("Reporter", before.ReporterId, before.ReporterName, entity.ReporterId, entity.Reporter?.Name);

            if (before.DueDate != entity.DueDate)
                changes["Due Date"] = (before.DueDate?.ToString("dd-MMM-yyyy") ?? "None", entity.DueDate?.ToString("dd-MMM-yyyy") ?? "None");
            if (before.Title != entity.Title)
                changes["Title"] = (before.Title ?? "", entity.Title ?? "");

            await _backlogItemService.LogEditHistoryAsync(entity.Id, changes, loggedUser.Id);

            //status change: record the status log and fire the resolved/reopened email
            if (before.StatusId != entity.StatusId)
                await _backlogItemService.LogStatusChangeAsync(entity, entity.StatusId, loggedUser.Id, before.StatusName, entity.Status?.Name);

            //upsert each custom field value (also records history)
            foreach (var fieldValue in fieldValues)
            {
                await _backlogItemService.InsertFieldValueAsync(entity.Id, fieldValue.CustomFieldId, fieldValue.Value);
            }

            await _userActivityService.InsertAsync("BackLog", string.Format(await _localizationService.GetResourceAsync("Log.RecordUpdated"), entity.Code), entity);

            return RedirectToAction("Index");
        }

        if (!fieldValuesErrors)
            ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Error.UnableToUpdateTask"));

        await InitModelAsync(model);
        model.CanEdit = await _permissionService.AuthorizeAsync(PermissionProvider.WorkItem.MANAGE_BACKLOGLOG);
        model.CanDelete = await _permissionService.AuthorizeAsync(PermissionProvider.WorkItem.MANAGE_BACKLOGLOG);

        return View(model);
    }

    [HttpPost("DeleteBacklog")]
    [ValidateAntiForgeryToken]  
    [CheckPermission(PermissionProvider.WorkItem.MANAGE_BACKLOGLOG)]
    public async Task<IActionResult> DeleteBacklogAsync(int id)
    {

        // check permission
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.WorkItem.MANAGE_BACKLOGLOG))
            return Forbid();

        var backlog = await _backlogItemService.GetByIdAsync(id);
        if (backlog == null)
            return NotFound();

        backlog.Deleted = true;
        await _backlogItemService.UpdateAsync(backlog);
        
        return Json(new
        {
            success = true,
            message = true ? "Backlog File deleted!" : "Oops, unable to delete the file!"
        });

    }

    public async Task<IActionResult> Filter(int filterMode)
    {
        var model = new BacklogModel();
        await InitFilterModelAsync(model, filterMode);

        return PartialView(model);
    }

    #endregion

    #region Actions For Documents

    [CheckPermission(PermissionProvider.WorkItem.MANAGE_BACKLOGLOG)]
    public async Task<IActionResult> Documents(int id)
    {
        var loggedUser = await _workContext.GetCurrentUserAsync();
        if (!await _backlogItemService.CanAccessAsync(id, loggedUser.Id))
            return AccessDeniedPartial();

        var data = await _backlogItemService.GetAllDocumentAsync(id);
        var model = data.Select(s => new BacklogDocumentGridModel
        {
            Id = s.Id,
            Name = s.Document.FileName,
            ContentType = s.Document.ContentType,
            Extension = s.Document.Extension,
            FileSize = s.Document.FileSize
        }).ToList();

        return PartialView(model);
    }

    [HttpPost]
    [CheckPermission(PermissionProvider.WorkItem.MANAGE_BACKLOGLOG)]
    public async Task<IActionResult> UploadDocument(IFormFile file, int reference = 0)
    {
        if (file == null)
            return Json(new { success = false, message = "No file uploaded" });

        if (!_documentService.IsAllowed(file, out var rejectReason))
            return Json(new { success = false, message = rejectReason });

        var loggedUser = await _workContext.GetCurrentUserAsync();

        //an attachment may only be added to an item the caller can already reach
        if (reference > 0 && !await _backlogItemService.CanAccessAsync(reference, loggedUser.Id))
            return AccessDeniedDataRead();

        var document = await _documentService.InsertAsync(file);

        if (document == null || document.Id == 0)
            return Json(new { success = false, message = "Wrong file format" });

        if (reference > 0)
            await _backlogItemService.InsertDocumentAsync(reference, document.Id, loggedUser.Id);

        return Json(new
        {
            success = true,
            token = document.Id
        });
    }

    [CheckPermission(PermissionProvider.WorkItem.MANAGE_BACKLOGLOG)]
    public async Task<IActionResult> ViewDocument(int token)
    {
        var loggedUser = await _workContext.GetCurrentUserAsync();
        if (!await _backlogItemService.CanAccessDocumentAsync(token, loggedUser.Id))
            return AccessDenied();

        var document = await _backlogItemService.GetDocumentByIdAsync(token);

        if (document == null)
            return BadRequest(new { message = "Unable to download the file" });

        //never echo the stored content type back - a text/html attachment would
        //otherwise execute in the origin. Force a download instead.
        Response.Headers.XContentTypeOptions = "nosniff";

        var fileName = $"{document.FileName}{document.Extension}";

        return File(document.FileData, MediaTypeNames.Application.Octet, fileName);
    }

    [HttpPost]
    [CheckPermission(PermissionProvider.WorkItem.MANAGE_BACKLOGLOG)]
    public async Task<IActionResult> DeleteDocument(int token)
    {
        var loggedUser = await _workContext.GetCurrentUserAsync();

        if (!await _backlogItemService.CanAccessDocumentAsync(token, loggedUser.Id))
            return AccessDeniedDataRead();

        var status = await _backlogItemService.DeleteDocumentAsync(token, loggedUser.Id);

        return Json(new
        {
            success = status,
            message = status ? "File deleted!" : "Oops, unable to delete the file!"
        });
    }

    #endregion

    #region Actions For History

    [CheckPermission(PermissionProvider.WorkItem.MANAGE_BACKLOGLOG)]
    public async Task<IActionResult> History(int id)
    {
        var loggedUser = await _workContext.GetCurrentUserAsync();
        if (!await _backlogItemService.CanAccessAsync(id, loggedUser.Id))
            return AccessDeniedPartial();

        var data = await _backlogItemService.GetAllHistoryAsync(id);
        var model = data.Select(s => new BacklogCommentGridModel
        {
            CommentById = s.CreatedById,
            CommentBy = s.CreatedBy.Name,
            CommentOn = s.CreatedOn,
            Comment = s.Comment,
            SystemComment = s.SystemComment
        }).ToList();

        return PartialView(model);
    }

    #endregion

    #region Actions For Comments

    [CheckPermission(PermissionProvider.WorkItem.MANAGE_BACKLOGLOG)]
    public async Task<IActionResult> Comments(int id)
    {
        var loggedUser = await _workContext.GetCurrentUserAsync();
        if (!await _backlogItemService.CanAccessAsync(id, loggedUser.Id))
            return AccessDeniedPartial();

        var data = await _backlogItemService.GetAllCommentsAsync(id);
        var model = new BacklogCommentModel()
        {
            BackLogId = id,
            CurrentUserId = loggedUser.Id,
            Comments = data.Select(s => new BacklogCommentGridModel
            {
                CommentById = s.CreatedById,
                CommentBy = s.CreatedBy.Name,
                CommentOn = s.CreatedOn,
                Comment = s.Comment,
                SystemComment = s.SystemComment
            }).ToList()
        };

        return PartialView(model);
    }

    [HttpPost]
    [CheckPermission(PermissionProvider.WorkItem.MANAGE_BACKLOGLOG)]
    public async Task<IActionResult> CommentCreate(int id, string comment)
    {
        var loggedUser = await _workContext.GetCurrentUserAsync();

        if (!await _backlogItemService.CanAccessAsync(id, loggedUser.Id))
            return AccessDeniedDataRead();

        await _backlogItemService.InsertCommentAsync(id, comment, loggedUser.Id);

        return Json(new
        {
            success = true,
            commentbyid = loggedUser.Id,
            commentby = loggedUser.Name,
            commenton = DateTime.Now.ToString("dd-MM-yyyy hh:mm")
        });
    }

    #endregion

    #region Data

    [HttpPost]
    [CheckPermission(PermissionProvider.WorkItem.MANAGE_BACKLOGLOG)]
    public async Task<IActionResult> DataRead(DataTableRequest request,
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
        var model = await GetProjectAccess();
        var loggedUser = await _workContext.GetCurrentUserAsync();
        var lastAccessedProjectId = await _genericAttributeService.GetAttributeAsync<int>(loggedUser, Constant.ActiveProjectSession);
        IList<Project> accessibleProjects = [];

        if (lastAccessedProjectId <= 0)
        {
            accessibleProjects = await _userService.GetAllAccessibleProjects(loggedUser.Id);
        }

        var data = await _backlogItemService.GetPagedListAsync(loggedUser.Id,
            lastAccessedProjectId,
            createdby,
            module,
            subModule,
            taskType,
            severity,
            reporter,
            assignee,
            status,
            sprint,
            accessibleProjects.Any() ? accessibleProjects.Select(x => x.Id).ToArray() : [],
            request.Start,
            request.Length,
            request.SortColumn,
            request.SortDirection,
            model.CanViewOthersTask);

        return Json(new
        {
            request.Draw,
            data = data.Select(x => new BacklogItemGridModel
            {
                Id = x.Id,
                Title = x.Title,
                DueDate = x.DueDate,
                Project = x.Project.Name,
                Module = x.Module != null ? x.Module.Name : "-",
                SubModule = x.SubModule != null ? x.SubModule.Name : "-",
                Sprint = x.Sprint?.Name,
                Assignee = x.Assignee != null ? x.Assignee.Name : "Unassigned",
                SubTaskCount = x.SubTaskCount,
                CreatedBy = x.CreatedBy.Name,
                CreatedOn = x.CreatedOn,
                TaskType = _mapper.Map<TaskTypeModel>(x.TaskType),
                Severity = _mapper.Map<SeverityModel>(x.Severity),
                Status = _mapper.Map<StatusModel>(x.Status)
            }),
            recordsFiltered = data.TotalCount,
            recordsTotal = data.TotalCount
        });
    }

    #endregion

    #region Exporting

    public async Task<IActionResult> ExportToExcel(int[] createdby,
        int[] module,
        int[] subModule,
        int[] taskType,
        int[] severity,
        int[] reporter,
        int[] assignee,
        int[] status,
        int[] sprint,
        string groups,
        string columns)
    {
        var model = await GetProjectAccess();
        var loggedUser = await _workContext.GetCurrentUserAsync();
        var lastAccessedProjectId = await _genericAttributeService.GetAttributeAsync<int>(loggedUser, Constant.ActiveProjectSession);
        IList<Project> accessibleProjects = [];

        if (lastAccessedProjectId <= 0)
        {
            accessibleProjects = await _userService.GetAllAccessibleProjects(loggedUser.Id);
        }
        columns = "Id,Title," + columns;
        var visibleColumns = string.IsNullOrWhiteSpace(columns) ? [] : columns.Split(',');
        var groupByColumns = string.IsNullOrWhiteSpace(groups) ? [] : groups.Split(',');

        var entities = await _backlogItemService.GetAllForExcelExportAsync(loggedUser.Id,
            lastAccessedProjectId,
            createdby,
            module,
            subModule,
            taskType,
            severity,
            reporter,
            assignee,
            status,
            sprint,
            accessibleProjects.Any() ? accessibleProjects.Select(x => x.Id).ToArray() : [],
            groupByColumns,
            model.CanViewOthersTask);

        var data = entities.Select(x => new BacklogExportModel
        {
            Id = x.Id,
            Title = x.Title,
            DueDate = x.DueDate != null ? "T" : "",
            Project = x.Project.Name,
            Module = x.Module != null ? x.Module.Name : "-",
            SubModule = x.SubModule != null ? x.SubModule.Name : "-",
            Sprint = x.Sprint?.Name,
            Assignee = x.Assignee != null ? x.Assignee.Name : "Unassigned",
            SubTaskCount = x.SubTaskCount,
            CreatedBy = x.CreatedBy.Name,
            CreatedOn = x.CreatedOn.ToString("dd-MMM-yyyy"),
            TaskType = x.TaskType.Name,
            Severity = x.Severity.Name,
            Status = x.Status.Name
        });

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Tasks");

            int row = 1; // Start row
            int startRow = 2; // Data start row
            int groupLevel = 1; // Track grouping level

            // Add headers
            for (int i = 0; i < visibleColumns.Length; i++)
            {
                worksheet.Cells[row, i + 1].Value = visibleColumns[i];
                worksheet.Cells[row, i + 1].Style.Font.Bold = true;
            }

            row++;

            if (groupByColumns.Any())
            {
                string lastGroup = "";
                foreach (var item in data)
                {
                    string currentGroup = "";
                    foreach (var groupCol in groupByColumns)
                    {
                        var value = item.GetType().GetProperty(groupCol)?.GetValue(item, null);
                        string groupValue = value == null ? "Unassigned" : value.ToString();
                        currentGroup += groupValue + " - ";
                    }

                    currentGroup = currentGroup.TrimEnd('-', ' ');

                    // Insert group header when group changes
                    if (currentGroup != lastGroup)
                    {
                        worksheet.Cells[row, 1].Value = currentGroup;
                        worksheet.Cells[row, 1].Style.Font.Bold = true;
                        worksheet.Row(row).OutlineLevel = groupLevel;
                        worksheet.Row(row).Collapsed = true;
                        row++;
                        lastGroup = currentGroup;
                    }

                    // Insert row data
                    for (int i = 0; i < visibleColumns.Length; i++)
                    {
                        var value = item.GetType().GetProperty(visibleColumns[i])?.GetValue(item, null);
                        worksheet.Cells[row, i + 1].Value = value ?? "Unassigned"; // Handle NULLs
                    }

                    worksheet.Row(row).OutlineLevel = groupLevel + 1; // Indent grouped data
                    row++;
                }
            }
            else
            {
                // No grouping applied, insert plain rows
                foreach (var item in data)
                {
                    for (int i = 0; i < visibleColumns.Length; i++)
                    {
                        var value = item.GetType().GetProperty(visibleColumns[i])?.GetValue(item, null);
                        worksheet.Cells[row, i + 1].Value = value ?? "Unassigned"; // Handle NULLs
                    }
                    row++;
                }
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Grouped_Tasks.xlsx");
        }
    }

    #endregion

    #region Helper

    private async Task<BacklogPageModel> GetProjectAccess()
    {
        var loggedUser = await _workContext.GetCurrentUserAsync();
        var lastAccessedProjectId = await _genericAttributeService.GetAttributeAsync<int>(loggedUser, Constant.ActiveProjectSession);
        var projectMap = await _userService.GetProjectMapping(loggedUser.Id, lastAccessedProjectId);

        //accessible projects power the contextual switcher on the backlog page
        var accessibleProjects = await _userService.GetAllAccessibleProjects(loggedUser.Id);

        return new BacklogPageModel
        {
            ProjectId = projectMap != null ? projectMap.Id : 0,
            ActiveProjectId = lastAccessedProjectId,
            AvailableProjects = accessibleProjects
                .Select(p => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = p.Name,
                    Value = p.Id.ToString(),
                    Selected = p.Id == lastAccessedProjectId
                })
                .ToList(),
            CanReport = projectMap != null && projectMap.CanReport,
            CanEdit = projectMap != null && projectMap.CanEdit,
            CanClose = projectMap != null && projectMap.CanClose,
            CanDelete = projectMap != null && projectMap.CanDelete,
            CanViewOthersTask = projectMap != null && projectMap.CanViewOthersTask,
            CanEditOthersTask = projectMap != null && projectMap.CanEditOthersTask
        };
    }

    private async Task InitModelAsync(BacklogModel model)
    {
        var loggedUser = await _workContext.GetCurrentUserAsync();
        var lastAccessedProjectId = await _genericAttributeService.GetAttributeAsync<int>(loggedUser, Constant.ActiveProjectSession);
        var projects = await _projectService.GetAllAccessibleAsync(loggedUser.Id);
        var taskTypes = await _taskTypeService.GetAllActiveAsync();
        var reporters = await _reporterService.GetAllActiveAsync();
        var severities = await _severityService.GetAllActiveAsync();
        var assignees = await _userService.GetAllActiveByProjectAsync(lastAccessedProjectId);
        var status = await _statusService.GetAllActiveAsync();
        var sprints = await _sprintService.GetAllActiveByProjectAsync(lastAccessedProjectId);

        if (model.Id > 0)
        {
            var access = await GetProjectAccess();
            model.CanEdit = model.AssigneeId == loggedUser.Id || access.CanEditOthersTask;
            model.CanDelete = access.CanDelete;
        }
        foreach (var item in taskTypes)
        {
            model.AvailableTaskTypes.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString(),
                Selected = item.Id == model.TaskTypeId
            });
        }

        foreach (var item in reporters)
        {
            model.AvailableReporters.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString(),
                Selected = item.Id == model.ReporterId
            });
        }

        foreach (var item in severities)
        {
            model.AvailableSeverities.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString(),
                Selected = item.Id == model.SeverityId
            });
        }

        foreach (var item in projects)
        {
            model.AvailableProjects.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString(),
                Selected = item.Id == model.ProjectId
            });
        }

        if (model.ProjectId <= 0)
            model.ProjectId = lastAccessedProjectId;

        if (model.ProjectId > 0)
        {
            var modules = await _moduleService.GetAllActiveByProjectAsync(model.ProjectId);
            foreach (var item in modules)
            {
                model.AvailableModules.Add(new SelectListItem
                {
                    Text = item.Name,
                    Value = item.Id.ToString(),
                    Selected = item.Id == model.ModuleId
                });
            }
        }

        if (model.ModuleId > 0)
        {
            var subModules = await _subModuleService.GetAllActiveByModuleAsync(model.ModuleId.ToInt());
            foreach (var item in subModules)
            {
                model.AvailableSubModules.Add(new SelectListItem
                {
                    Text = item.Name,
                    Value = item.Id.ToString(),
                    Selected = item.Id == model.SubModuleId
                });
            }
        }

        var mySelf = assignees.FirstOrDefault(x => x.Id == loggedUser.Id);
        if (mySelf != null)
        {
            assignees = assignees.Where(x => x.Id != mySelf.Id).ToList();
            model.AvailableAssignees.Add(new SelectListItem
            {
                Text = "Myself",
                Value = mySelf.Id.ToString(),
                Selected = mySelf.Id == model.AssigneeId
            });
        }

        foreach (var item in assignees)
        {
            model.AvailableAssignees.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString(),
                Selected = item.Id == model.AssigneeId
            });
        }

        foreach (var item in sprints)
        {
            model.AvailableSprints.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString(),
                Selected = item.Id == model.SprintId
            });
        }

        var newStatus = status.FirstOrDefault(x => x.GroupId == (int)StatusGroupEnum.New);

        if (model.Id == 0)
        {
            if (newStatus != null)
            {
                model.AvailableStatus.Add(new SelectListItem
                {
                    Text = newStatus.Name,
                    Value = newStatus.Id.ToString(),
                    Selected = true
                });
            }
        }
        else
        {
            var mapping = await _projectService.GetMappingForUserAsync(lastAccessedProjectId, loggedUser.Id);
            var filteredStatus = status;
            if (!mapping.CanReOpen)
            {
                filteredStatus = filteredStatus.Where(x => x.GroupId != StatusGroupEnum.ReOpened.ToInt()).ToList();
            }
            if (!mapping.CanClose)
            {
                filteredStatus = filteredStatus.Where(x => x.GroupId != StatusGroupEnum.Resolved.ToInt()).ToList();
            }

            foreach (var item in filteredStatus)
            {
                model.AvailableStatus.Add(new SelectListItem
                {
                    Text = item.Name,
                    Value = item.Id.ToString(),
                    Selected = item.Id == model.StatusId
                });
            }
        }
    }

    private async Task InitFilterModelAsync(BacklogModel model, int filterMode)
    {
        var loggedUser = await _workContext.GetCurrentUserAsync();
        var lastAccessedProjectId = await _genericAttributeService.GetAttributeAsync<int>(loggedUser, Constant.ActiveProjectSession);

        var taskTypes = await _taskTypeService.GetAllActiveAsync();
        var reporters = await _reporterService.GetAllActiveAsync();
        var severities = await _severityService.GetAllActiveAsync();
        var modules = await _moduleService.GetAllActiveByProjectAsync(lastAccessedProjectId);
        var subModules = await _subModuleService.GetAllActiveAsync();
        var assignees = await _userService.GetAllActiveByProjectAsync(lastAccessedProjectId);
        var status = await _statusService.GetAllActiveAsync();
        var sprints = await _sprintService.GetAllActiveAsync();

        model.AvailableTaskTypes = [];
        model.AvailableReporters = [];
        model.AvailableSeverities = [];
        model.AvailableModules = [];
        model.AvailableSubModules = [];
        model.AvailableSprints = [];
        model.AvailableAssignees = [];
        model.AvailableStatus = [];

        foreach (var item in taskTypes)
        {
            model.AvailableTaskTypes.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString()
            });
        }

        foreach (var item in reporters)
        {
            model.AvailableReporters.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString()
            });
        }

        foreach (var item in severities)
        {
            model.AvailableSeverities.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString()
            });
        }

        foreach (var item in modules)
        {
            model.AvailableModules.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString()
            });
        }

        foreach (var item in subModules)
        {
            model.AvailableSubModules.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString()
            });
        }

        var mySelf = assignees.FirstOrDefault(x => x.Id == loggedUser.Id);
        if (mySelf != null)
        {
            assignees = assignees.Where(x => x.Id != mySelf.Id).ToList();
            model.AvailableAssignees.Add(new SelectListItem
            {
                Text = "Myself",
                Value = mySelf.Id.ToString(),
                Selected = filterMode == 1
            });
        }

        foreach (var item in assignees)
        {
            model.AvailableAssignees.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString(),
                Selected = item.Id == model.AssigneeId
            });
        }

        foreach (var item in sprints)
        {
            model.AvailableSprints.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString()
            });
        }

        var selected = false;

        foreach (var item in status)
        {
            switch (filterMode)
            {
                case 2:
                    selected = item.GroupId == StatusGroupEnum.ReOpened.ToInt();
                    break;
                case 3:
                    selected = item.GroupId == StatusGroupEnum.Resolved.ToInt();
                    break;
                default:
                    selected = false;
                    break;
            }

            model.AvailableStatus.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString(),
                Selected = selected
            });
        }
    }

    #endregion
}