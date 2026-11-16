using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using Taskist.Core.Common;
using Taskist.Core.Domain.WorkItems;
using Taskist.Core.Enums;
using Taskist.Core.Extensions;
using Taskist.Service.Localization;
using Taskist.Service.Masters;
using Taskist.Service.Security;
using Taskist.Service.Users;
using Taskist.Service.WorkItems;
using Taskist.Web.Controllers.Common;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Helpers.Mapping;
using Taskist.Web.Models.Common;
using Taskist.Web.Models.Datatable;
using Taskist.Web.Models.WorkItems;

namespace Taskist.Web.Controllers.WorkItems;

[CheckPermission(nameof(PermissionProvider.ManageRelease))]
public class ReleasePlanController : BaseController
{
    #region Fields

    protected readonly IReleaseService _releaseService;
    protected readonly IReleaseVersionService _releaseVersionService;
    protected readonly IUserService _userService;
    protected readonly IBacklogService _backlogItemService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IGenericAttributeService _genericAttributeService;
    protected readonly IWorkContext _workContext;
    protected readonly WorkItemsMapper _mapper;

    #endregion

    #region Ctor

    public ReleasePlanController(IReleaseService releaseService,
        IReleaseVersionService releaseVersionService,
        IUserService userService,
        IBacklogService backlogItemService,
        ILocalizationService localizationService,
        IGenericAttributeService genericAttributeService,
        IWorkContext workContext,
        WorkItemsMapper mapper)
    {
        _releaseService = releaseService;
        _releaseVersionService = releaseVersionService;
        _userService = userService;
        _backlogItemService = backlogItemService;
        _localizationService = localizationService;
        _genericAttributeService = genericAttributeService;
        _workContext = workContext;
        _mapper = mapper;

    }

    #endregion

    #region Actions

    public async Task<IActionResult> Index()
    {
        return View(new ReleasePlanIndexModel
        {
            Projects = await _userService.GetAllAccessibleProjects(UserId),
            MismatchCount = (await _backlogItemService.GetReleaseProjectMismatchesAsync((await GetAccessibleProjectIdsAsync()).ToArray())).Count
        });
    }

    public async Task<IActionResult> Create()
    {
        var model = new ReleaseModel
        {
            ReleaseType = ReleaseType.Full.ToInt(),
            VersionAuto = true
        };
        var suggestion = await _releaseVersionService.GetNextAsync(ReleaseType.Full, []);
        model.Name = suggestion.Version;
        model.VersionFormat = suggestion.Format;
        model.AvailableBaseReleases = await GetBaseReleaseItemsAsync([]);
        await InitModelAsync(model);

        return PartialView(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(ReleaseModel model)
    {
        if (ModelState.IsValid)
        {
            var accessibleIds = await GetAccessibleProjectIdsAsync();
            var projectIds = ResolveProjectIds(model, accessibleIds, []);
            model.ProjectId = projectIds.Count == 1 ? projectIds[0] : null;

            if (model.VersionAuto)
            {
                var releaseType = Enum.IsDefined(typeof(ReleaseType), model.ReleaseType) ? (ReleaseType)model.ReleaseType : ReleaseType.Full;
                model.Name = (await _releaseVersionService.GetNextAsync(releaseType, projectIds, model.BaseReleaseId ?? 0)).Version;
            }

            model.Name = model.Name.Trim();
            var entity = _mapper.ToEntity(model);

            await _releaseService.InsertAsync(entity);
            await _releaseService.SaveProjectsAsync(entity.Id, projectIds);

            return Json(new JsonResponseModel
            {
                Status = HttpStatusCode.Success,
                Message = await _localizationService.GetResourceAsync("Message.SaveSuccess")
            });
        }

        return Json(new JsonResponseModel
        {
            Status = ModelState.IsValid ? HttpStatusCode.InternalServerError : HttpStatusCode.ValidationError,
            Message = await _localizationService.GetResourceAsync("Error.Failed"),
            Errors = ModelState.AllErrors()
        });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _releaseService.GetByIdAsync(id);
        if (entity == null)
            return NoDataPartial();

        var model = _mapper.ToModel(entity);
        model.SelectedProjectIds = (await _releaseService.GetProjectsAsync(entity.Id)).Select(x => x.ProjectId).ToList();
        model.AllProjects = model.SelectedProjectIds.Count == 0;
        await InitModelAsync(model);

        return PartialView(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(ReleaseModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = await _releaseService.GetByIdAsync(model.Id);
            if (entity == null)
                return Json(new JsonResponseModel
                {
                    Status = HttpStatusCode.NoData,
                    Message = await _localizationService.GetResourceAsync("FormNoData.Description")
                });

            var accessibleIds = await GetAccessibleProjectIdsAsync();
            var existingIds = (await _releaseService.GetProjectsAsync(entity.Id)).Select(x => x.ProjectId).ToList();
            var projectIds = ResolveProjectIds(model, accessibleIds, existingIds);

            var releaseTasks = await _backlogItemService.GetAllByReleaseAsync(entity.Id);
            var releasing = model.Status == ReleaseStatus.Released.ToInt() && entity.Status != ReleaseStatus.Released.ToInt();
            var openTasks = releasing ? releaseTasks.Where(IsOpenTask).ToList() : [];
            var carryOverId = model.CarryOverReleaseId ?? 0;

            if (openTasks.Count > 0)
            {
                if (!model.ConfirmOpenTasks)
                    return Json(new JsonResponseModel
                    {
                        Status = HttpStatusCode.ValidationError,
                        Message = string.Format(await _localizationService.GetResourceAsync("ReleaseCarryOver.OpenTasksMsg"), openTasks.Count)
                    });

                if (carryOverId > 0)
                {
                    var target = await _releaseService.GetByIdAsync(carryOverId);
                    var openProjectIds = openTasks.Select(x => x.ProjectId).Distinct().ToList();
                    if (target == null || target.Id == entity.Id || target.Deleted || target.Status != ReleaseStatus.Planning.ToInt()
                        || !await _releaseService.IsAvailableForAllProjectsAsync(carryOverId, openProjectIds))
                        return Json(new JsonResponseModel
                        {
                            Status = HttpStatusCode.ValidationError,
                            Message = await _localizationService.GetResourceAsync("ReleaseCarryOver.InvalidTargetMsg")
                        });
                }
            }

            var movingIds = carryOverId > 0 ? openTasks.Select(x => x.Id).ToHashSet() : [];

            if (projectIds.Count > 0)
            {
                var orphaned = releaseTasks
                    .Where(x => !movingIds.Contains(x.Id))
                    .Where(x => !projectIds.Contains(x.ProjectId))
                    .GroupBy(x => x.Project?.Name ?? x.ProjectId.ToString())
                    .ToList();

                if (orphaned.Count > 0)
                {
                    var message = string.Format(await _localizationService.GetResourceAsync("ReleaseModel.ProjectRemoval.BlockedMsg"),
                        orphaned.Sum(g => g.Count()), string.Join(", ", orphaned.Select(g => $"{g.Key} ({g.Count()})")));

                    return Json(new JsonResponseModel
                    {
                        Status = HttpStatusCode.ValidationError,
                        Message = message,
                        Errors = [new ErrorModel(nameof(ReleaseModel.SelectedProjectIds), message)]
                    });
                }
            }

            model.ProjectId = projectIds.Count == 1 ? projectIds[0] : null;
            model.Name = model.Name.Trim();
            _mapper.Update(model, entity);

            await _releaseService.UpdateAsync(entity);
            await _releaseService.SaveProjectsAsync(entity.Id, projectIds);

            var successMessage = await _localizationService.GetResourceAsync("Message.UpdateSuccess");
            if (movingIds.Count > 0)
            {
                await _backlogItemService.UpdateBatchAsync(string.Join(",", movingIds), null, null, null,
                    null, null, null, carryOverId, null, null, null);
                successMessage = string.Format(await _localizationService.GetResourceAsync("ReleaseCarryOver.MovedMsg"), movingIds.Count);
            }

            return Json(new JsonResponseModel
            {
                Status = HttpStatusCode.Success,
                Message = successMessage
            });
        }

        return Json(new JsonResponseModel
        {
            Status = ModelState.IsValid ? HttpStatusCode.InternalServerError : HttpStatusCode.ValidationError,
            Message = await _localizationService.GetResourceAsync("Error.Failed"),
            Errors = ModelState.AllErrors()
        });
    }

    [HttpGet]
    public async Task<IActionResult> NextVersion(int releaseType, bool allProjects = true, int[]? projectIds = null, int baseReleaseId = 0)
    {
        var ids = await ResolveScopeAsync(allProjects, projectIds);
        var type = Enum.IsDefined(typeof(ReleaseType), releaseType) ? (ReleaseType)releaseType : ReleaseType.Full;

        var suggestion = await _releaseVersionService.GetNextAsync(type, ids, baseReleaseId);
        var bases = await GetBaseReleaseItemsAsync(ids);

        return Json(new
        {
            version = suggestion.Version,
            format = suggestion.Format,
            projectFormat = suggestion.ProjectFormat,
            bases = bases.Select(x => new { value = x.Value, text = x.Text })
        });
    }

    [HttpGet]
    public async Task<IActionResult> CheckVersion(string? name, bool allProjects = true, int[]? projectIds = null, int id = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Json(new { matchesFormat = true, exists = false });

        var ids = await ResolveScopeAsync(allProjects, projectIds);
        var check = await _releaseVersionService.CheckAsync(name.Trim(), ids, id);

        return Json(new
        {
            matchesFormat = check.MatchesFormat,
            exists = check.Exists,
            format = check.Format
        });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _releaseService.GetByIdAsync(id);
        if (entity == null)
            return Json(new JsonResponseModel
            {
                Status = HttpStatusCode.NoData,
                Message = await _localizationService.GetResourceAsync("FormNoData.Description")
            });

        await _releaseService.DeleteAsync(entity);

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCode.Success,
            Message = await _localizationService.GetResourceAsync("Message.DeleteSuccess")
        });
    }

    #endregion

    #region Actions For Carry Over

    [HttpGet]
    public async Task<IActionResult> OpenTasks(int id)
    {
        var openTasks = (await _backlogItemService.GetAllByReleaseAsync(id)).Where(IsOpenTask).ToList();
        var projectIds = openTasks.Select(x => x.ProjectId).Distinct().ToList();

        var targets = openTasks.Count == 0
            ? []
            : (await _releaseService.GetAllActiveForAllProjectsAsync(projectIds))
                .Where(x => x.Id != id)
                .OrderBy(x => x.ReleaseDate).ThenBy(x => x.Name)
                .Select(x => new { text = $"{x.Name} ({x.ReleaseDate:dd-MMM-yyyy})", value = x.Id.ToString() })
                .ToList();

        return Json(new { count = openTasks.Count, targets });
    }

    #endregion

    #region Actions For Mismatches

    public async Task<IActionResult> Mismatches()
    {
        var tasks = await _backlogItemService.GetReleaseProjectMismatchesAsync((await GetAccessibleProjectIdsAsync()).ToArray());
        var model = tasks
            .GroupBy(x => new { x.ReleaseId, x.ProjectId })
            .Select(g =>
            {
                var first = g.First();
                return new ReleaseMismatchGroupModel
                {
                    ReleaseId = g.Key.ReleaseId ?? 0,
                    Release = first.Release?.Name,
                    MappedProjects = string.Join(", ", (first.Release?.ReleaseProjects ?? [])
                        .Select(p => p.Project?.Name).Where(n => !string.IsNullOrEmpty(n)).OrderBy(n => n)),
                    ProjectId = g.Key.ProjectId,
                    Project = first.Project?.Name,
                    CanAddProject = first.Release != null && !first.Release.Deleted,
                    Tasks = g.OrderBy(x => x.Id).Select(x => new ReleaseMismatchModel
                    {
                        TaskId = x.Id,
                        Title = x.Title,
                        Project = x.Project?.Name,
                        Release = x.Release?.Name,
                        Status = x.Status?.Name
                    }).ToList()
                };
            })
            .OrderByDescending(x => x.Tasks.Count)
            .ThenBy(x => x.Release)
            .ToList();

        return PartialView(model);
    }

    [HttpPost]
    public async Task<IActionResult> AddProjectToRelease(int releaseId, int projectId)
    {
        var release = await _releaseService.GetByIdAsync(releaseId);
        var accessibleIds = await GetAccessibleProjectIdsAsync();
        if (release == null || release.Deleted || !accessibleIds.Contains(projectId))
            return Json(new JsonResponseModel
            {
                Status = HttpStatusCode.ValidationError,
                Message = await _localizationService.GetResourceAsync("FormNoData.Description")
            });

        var projectIds = (await _releaseService.GetProjectsAsync(releaseId)).Select(x => x.ProjectId).ToList();
        if (projectIds.Count > 0 && !projectIds.Contains(projectId))
        {
            projectIds.Add(projectId);
            await _releaseService.SaveProjectsAsync(releaseId, projectIds);

            release.ProjectId = null;
            await _releaseService.UpdateAsync(release);
        }

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCode.Success,
            Message = await _localizationService.GetResourceAsync("ReleaseMismatch.ProjectAdded")
        });
    }

    [HttpPost]
    public async Task<IActionResult> ClearMismatchedReleases(int[] ids)
    {
        var stillMismatched = (await _backlogItemService.GetReleaseProjectMismatchesAsync((await GetAccessibleProjectIdsAsync()).ToArray()))
            .Select(x => x.Id)
            .Intersect(ids ?? [])
            .ToList();

        if (stillMismatched.Count == 0)
            return Json(new JsonResponseModel
            {
                Status = HttpStatusCode.ValidationError,
                Message = await _localizationService.GetResourceAsync("ReleaseMismatch.NothingToClear")
            });

        await _backlogItemService.UpdateBatchAsync(string.Join(",", stillMismatched), null, null, null,
            null, null, null, 0, null, null, null);

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCode.Success,
            Message = string.Format(await _localizationService.GetResourceAsync("ReleaseMismatch.Cleared"), stillMismatched.Count)
        });
    }

    #endregion

    #region Data

    [HttpPost]
    public async Task<IActionResult> DataRead(DataTableRequest request, int projectId = 0,
        int[]? status = null, int[]? releaseType = null, string? fromDate = null, string? toDate = null)
    {
        var accessibleIds = (await _userService.GetAllAccessibleProjects(UserId)).Select(x => x.Id).ToArray();
        var scopeId = (projectId > 0 && accessibleIds.Contains(projectId)) ? projectId : 0;

        var data = await _releaseService.GetPagedListAsync(scopeId, scopeId <= 0 ? accessibleIds : [],
                request.SearchValue, request.Start, request.Length, request.SortColumn, request.SortDirection,
                status, releaseType, ParseFilterDate(fromDate), ParseFilterDate(toDate));

        var allProjectsLabel = await _localizationService.GetResourceAsync("ReleaseGrid.AllProjects");
        var viewAllIds = (await _userService.GetAllAccessibleProjects(UserId, fetchOnlyCanViewOthersTask: true)).Select(x => x.Id).ToArray();
        var progress = (await _backlogItemService.GetReleaseProjectProgressAsync(data.Select(r => r.Id).ToArray(), UserId, accessibleIds, viewAllIds))
            .GroupBy(p => p.ReleaseId)
            .ToDictionary(g => g.Key, g => g.OrderBy(p => p.Project).ToList());
        var model = new List<ReleaseGridModel>();
        foreach (var x in data)
        {
            var breakdown = progress.TryGetValue(x.Id, out var b) ? b : [];
            var projectNames = x.ReleaseProjects.Select(p => p.Project?.Name).Where(n => !string.IsNullOrEmpty(n)).OrderBy(n => n).ToList();

            var item = new ReleaseGridModel
            {
                Id = x.Id,
                ReleaseType = x.ReleaseType.ToString<ReleaseType>(),
                Name = x.Name,
                Description = x.Description,
                Project = projectNames.Count == 0 ? allProjectsLabel : string.Join(", ", projectNames),
                ProjectNames = projectNames,
                ReleaseDate = x.ReleaseDate,
                Status = x.Status.ToString<ReleaseStatus>(),
                TotalBacklog = breakdown.Sum(p => p.Total),
                CompletedBacklog = breakdown.Sum(p => p.Completed),
                ProjectProgress = breakdown
            };
            model.Add(item);
        }

        return Json(new
        {
            request.Draw,
            data = model,
            recordsFiltered = data.TotalCount,
            recordsTotal = data.TotalCount
        });
    }

    #endregion

    #region Helper

    private static DateOnly? ParseFilterDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return DateOnly.TryParseExact(value, "dd-MMM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? parsed
            : DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fallback) ? fallback : null;
    }

    private static bool IsOpenTask(Backlog task)
    {
        var group = task.Status?.GroupId ?? 0;
        return group != StatusGroup.Resolved.ToInt() && group != StatusGroup.Archived.ToInt();
    }

    private async Task<List<int>> GetAccessibleProjectIdsAsync()
    {
        return (await _userService.GetAllAccessibleProjects(UserId)).Select(x => x.Id).ToList();
    }

    private async Task<List<int>> ResolveScopeAsync(bool allProjects, int[]? projectIds)
    {
        if (allProjects)
            return [];

        var accessibleIds = await GetAccessibleProjectIdsAsync();
        return (projectIds ?? []).Where(accessibleIds.Contains).Distinct().ToList();
    }

    private async Task<IList<SelectListItem>> GetBaseReleaseItemsAsync(IList<int> projectIds)
    {
        return (await _releaseVersionService.GetPatchBasesAsync(projectIds))
            .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
            .ToList();
    }

    private static List<int> ResolveProjectIds(ReleaseModel model, IList<int> accessibleIds, IList<int> existingIds)
    {
        if (model.AllProjects)
            return [];

        var selected = (model.SelectedProjectIds ?? []).Where(accessibleIds.Contains);
        var preserved = existingIds.Where(x => !accessibleIds.Contains(x));

        return selected.Union(preserved).Distinct().ToList();
    }

    private async Task InitModelAsync(ReleaseModel model)
    {
        var projects = await _userService.GetAllAccessibleProjects(UserId);
        var selected = model.SelectedProjectIds ?? [];

        foreach (var item in projects.OrderBy(x => x.Name))
        {
            model.AvailableProjects.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString(),
                Selected = selected.Contains(item.Id)
            });
        }

        if (model.Id > 0)
        {
            foreach (var mapping in await _releaseService.GetProjectsAsync(model.Id))
            {
                if (model.AvailableProjects.Any(x => x.Value == mapping.ProjectId.ToString()))
                    continue;

                model.AvailableProjects.Add(new SelectListItem
                {
                    Text = mapping.Project?.Name ?? mapping.ProjectId.ToString(),
                    Value = mapping.ProjectId.ToString(),
                    Selected = true,
                    Disabled = true
                });
            }
        }
    }

    #endregion
}
