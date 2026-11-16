using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using Taskist.Core.Common;
using Taskist.Service.Logging;
using Taskist.Service.Security;
using Taskist.Service.Users;
using Taskist.Web.Controllers.Common;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Datatable;
using Taskist.Web.Models.Logging;

namespace Taskist.Web.Controllers.Configuration;

[CheckPermission(nameof(PermissionProvider.ViewAuditLog))]
public class ActivityLogController : BaseController
{
    #region Fields

    protected readonly IUserActivityService _activityService;
    protected readonly IAuditLogService _auditLogService;
    protected readonly IUserService _userService;

    #endregion

    #region Ctor

    public ActivityLogController(IUserActivityService activityService,
        IAuditLogService auditLogService,
        IUserService userService)
    {
        _activityService = activityService;
        _auditLogService = auditLogService;
        _userService = userService;
    }

    #endregion

    #region Actions

    public async Task<IActionResult> Index()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var users = await _userService.GetAllAsync();

        var model = new LogViewModel
        {
            From = today.AddDays(-7),
            To = today,
            Users = users.OrderBy(e => e.Name)
                .Select(e => new SelectListItem(e.Name, e.Id.ToString()))
                .ToList(),
            Types = new[] { ActivityType.Login, ActivityType.LoginFailed, ActivityType.Logout, ActivityType.AccessDenied, ActivityType.Export }
                .Select(t => new SelectListItem(CommonHelper.Beautify(t), t))
                .ToList(),
            Actions = new[] { "Added", "Modified", "Deleted" }
                .Select(a => new SelectListItem(a, a))
                .ToList()
        };

        return View(model);
    }

    public async Task<IActionResult> AuditDetail(int id)
    {
        var entity = await _auditLogService.GetByIdAsync(id);
        if (entity == null)
            return NoDataPartial();

        var user = entity.UserId.HasValue ? await _userService.GetByIdAsync(entity.UserId.Value) : null;

        var oldValues = ParseValues(entity.OldValues);
        var newValues = ParseValues(entity.NewValues);

        var model = new AuditDetailModel
        {
            Entity = entity.EntityName,
            EntityId = entity.EntityId,
            Action = entity.Action,
            User = user?.Name ?? (entity.UserId.HasValue ? $"#{entity.UserId}" : "System"),
            When = entity.CreatedOnUtc,
            Ip = entity.IpAddress,
            OldRaw = entity.OldValues,
            NewRaw = entity.NewValues
        };

        foreach (var field in oldValues.Keys.Union(newValues.Keys).OrderBy(k => k))
        {
            var oldVal = oldValues.TryGetValue(field, out var o) ? o : string.Empty;
            var newVal = newValues.TryGetValue(field, out var n) ? n : string.Empty;
            model.Changes.Add(new AuditChangeRowModel
            {
                Field = CommonHelper.Beautify(field),
                Old = oldVal,
                New = newVal,
                Changed = !string.Equals(oldVal, newVal, StringComparison.Ordinal)
            });
        }

        return PartialView("_AuditDetail", model);
    }

    #endregion

    #region Data

    [HttpPost]
    public async Task<IActionResult> ActivityDataRead(DataTableRequest request, DateOnly? from, DateOnly? to, int? userId, string type)
    {
        var (fromDt, toDt) = ResolveRange(from, to);
        var pageIndex = request.Length > 0 ? request.Start / request.Length : 0;

        var data = await _activityService.GetAllAsync(fromDt, toDt, userId, type, pageIndex: pageIndex, pageSize: request.Length);
        var nameById = await UserNamesAsync();

        return Json(new
        {
            request.Draw,
            data = data.Select(a => new
            {
                When = a.CreatedOnUtc.ToString("dd-MMM-yyyy HH:mm:ss"),
                User = nameById.TryGetValue(a.UserId, out var nm) ? nm : $"#{a.UserId}",
                Type = CommonHelper.Beautify(a.SystemName),
                a.Comment,
                Entity = string.IsNullOrEmpty(a.EntityName) ? "" : $"{a.EntityName}{(a.EntityId.HasValue ? " #" + a.EntityId : "")}",
                Ip = a.IpAddress
            }),
            recordsFiltered = data.TotalCount,
            recordsTotal = data.TotalCount
        });
    }

    [HttpPost]
    public async Task<IActionResult> AuditDataRead(DataTableRequest request, DateOnly? from, DateOnly? to, int? userId, string action)
    {
        var (fromDt, toDt) = ResolveRange(from, to);
        var pageIndex = request.Length > 0 ? request.Start / request.Length : 0;

        var data = await _auditLogService.GetAllAsync(fromDt, toDt, userId, action: action, pageIndex: pageIndex, pageSize: request.Length);
        var nameById = await UserNamesAsync();

        return Json(new
        {
            request.Draw,
            data = data.Select(a => new
            {
                a.Id,
                When = a.CreatedOnUtc.ToString("dd-MMM-yyyy HH:mm:ss"),
                User = a.UserId.HasValue && nameById.TryGetValue(a.UserId.Value, out var nm) ? nm : (a.UserId.HasValue ? $"#{a.UserId}" : "System"),
                a.Action,
                Entity = $"{a.EntityName}{(a.EntityId.HasValue ? " #" + a.EntityId : "")}",
                Ip = a.IpAddress
            }),
            recordsFiltered = data.TotalCount,
            recordsTotal = data.TotalCount
        });
    }

    #endregion

    #region Utilities

    private async Task<Dictionary<int, string>> UserNamesAsync()
    {
        return (await _userService.GetAllAsync()).ToDictionary(e => e.Id, e => e.Name);
    }

    private static (DateTime from, DateTime to) ResolveRange(DateOnly? from, DateOnly? to)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var f = from ?? today.AddDays(-7);
        var t = to ?? today;
        return (f.ToDateTime(TimeOnly.MinValue), t.ToDateTime(TimeOnly.MaxValue));
    }

    private static Dictionary<string, string> ParseValues(string json)
    {
        var result = new Dictionary<string, string>();
        if (string.IsNullOrWhiteSpace(json))
            return result;

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind == JsonValueKind.Object)
                foreach (var prop in doc.RootElement.EnumerateObject())
                    result[prop.Name] = prop.Value.ValueKind == JsonValueKind.Null ? string.Empty : prop.Value.ToString();
        }
        catch
        {
        }

        return result;
    }

    #endregion
}
