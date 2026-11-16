using Microsoft.AspNetCore.Mvc;
using Taskist.Core.Dtos.Git;
using Taskist.Core.Enums;
using Taskist.Service.Git;
using Taskist.Service.Localization;
using Taskist.Service.Security;
using Taskist.Service.Users;
using Taskist.Service.WorkItems;
using Taskist.Web.Controllers.Common;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;
using Taskist.Web.Models.WorkItems;

namespace Taskist.Web.Controllers.WorkItems;

[CheckPermission(nameof(PermissionProvider.ReviewMergeRequests))]
public class ReviewController : BaseController
{
    #region Fields

    protected readonly IMergeRequestReviewService _reviewService;
    protected readonly IGitRepositoryService _gitRepositoryService;
    protected readonly IGitAccountService _gitAccountService;
    protected readonly IUserService _userService;
    protected readonly IPermissionService _permissionService;
    protected readonly IBranchCleanupService _branchCleanupService;
    protected readonly IGitBranchService _gitBranchService;
    protected readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public ReviewController(IMergeRequestReviewService reviewService,
        IGitRepositoryService gitRepositoryService,
        IGitAccountService gitAccountService,
        IUserService userService,
        IPermissionService permissionService,
        IBranchCleanupService branchCleanupService,
        IGitBranchService gitBranchService,
        ILocalizationService localizationService)
    {
        _gitBranchService = gitBranchService;
        _branchCleanupService = branchCleanupService;
        _localizationService = localizationService;
        _reviewService = reviewService;
        _gitRepositoryService = gitRepositoryService;
        _gitAccountService = gitAccountService;
        _userService = userService;
        _permissionService = permissionService;
    }

    #endregion

    #region Pages

    public async Task<IActionResult> Index()
    {
        var user = await _userService.GetByIdAsync(UserId);
        var scope = await _reviewService.GetScopeAsync(user);
        var connectionIds = scope.Select(s => s.ConnectionId).Distinct().ToList();
        var model = new ReviewIndexModel
        {
            GitConfigured = await _gitRepositoryService.AnyConfiguredAsync(),
            Connections = (await _gitAccountService.GetStatusesAsync(user)).Where(c => connectionIds.Contains(c.ConnectionId)).ToList(),
            ProjectCount = scope.Select(s => s.ProjectId).Distinct().Count(),
            CanCleanupBranches = await _branchCleanupService.CanCleanupAsync(UserId),
            UnlinkedBranchCount = (await _gitBranchService.GetUnlinkedBranchesAsync(await _gitBranchService.GetBranchCaretakerProjectIdsAsync(UserId))).Count
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> UnlinkedBranches()
    {
        var projectIds = await _gitBranchService.GetBranchCaretakerProjectIdsAsync(UserId);
        return PartialView(await _gitBranchService.GetUnlinkedBranchesAsync(projectIds));
    }

    [HttpGet]
    public async Task<IActionResult> BranchCleanup()
    {
        return PartialView(await _branchCleanupService.GetCandidatesAsync(UserId));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteBranches(string ids)
    {
        var branchIds = (ids ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => int.TryParse(x, out var id) ? id : 0).Where(x => x > 0).Distinct().ToList();
        var result = await _branchCleanupService.DeleteAsync(UserId, branchIds);
        var removed = result.Deleted + result.AlreadyGone;
        return Json(new
        {
            status = removed > 0 || result.Failures.Count == 0 ? HttpStatusCode.Success : HttpStatusCode.ValidationError,
            message = string.Format(await _localizationService.GetResourceAsync("Backlog.Cleanup.Result"), result.Deleted, result.AlreadyGone),
            deleted = result.Deleted,
            alreadyGone = result.AlreadyGone,
            failures = result.Failures,
            needsConnection = result.NeedsConnection
        });
    }

    public async Task<IActionResult> Queue(bool refresh = false)
    {
        var user = await _userService.GetByIdAsync(UserId);
        return PartialView("_Queue", await _reviewService.GetQueueAsync(user, refresh));
    }

    public async Task<IActionResult> Detail(int repositoryId, int iid)
    {
        var user = await _userService.GetByIdAsync(UserId);
        var (detail, error) = await _reviewService.GetDetailAsync(user, repositoryId, iid);
        if (detail == null)
            return PartialView("_Error", error);

        return PartialView(new ReviewDetailViewModel
        {
            Detail = detail,
            CanMergePermission = await _permissionService.AuthorizeAsync(PermissionProvider.MergeMergeRequests.SystemName, user)
        });
    }

    public async Task<IActionResult> Changes(int repositoryId, int iid)
    {
        var user = await _userService.GetByIdAsync(UserId);
        var (changes, error) = await _reviewService.GetChangesAsync(user, repositoryId, iid);
        if (changes == null)
            return PartialView("_Error", error);

        return PartialView("_Changes", changes);
    }

    #endregion

    #region Actions

    [HttpPost]
    public async Task<JsonResponseModel> Approve(int repositoryId, int iid, bool approve = true)
    {
        var user = await _userService.GetByIdAsync(UserId);
        return ToJson(await _reviewService.SetApprovalAsync(user, repositoryId, iid, approve));
    }

    [HttpPost]
    public async Task<IActionResult> CheckConflicts()
    {
        var user = await _userService.GetByIdAsync(UserId);
        var (conflicting, notified) = await _reviewService.CheckConflictsAsync(user);
        return Json(new { status = HttpStatusCode.Success, conflicting, notified });
    }

    [HttpPost]
    public async Task<JsonResponseModel> Comment(int repositoryId, int iid, string body)
    {
        var user = await _userService.GetByIdAsync(UserId);
        return ToJson(await _reviewService.CommentAsync(user, repositoryId, iid, body));
    }

    [HttpPost]
    public async Task<JsonResponseModel> Reject(int repositoryId, int iid, string reason)
    {
        var user = await _userService.GetByIdAsync(UserId);
        return ToJson(await _reviewService.RejectAsync(user, repositoryId, iid, reason));
    }

    [HttpPost]
    public async Task<JsonResponseModel> RequestChanges(int repositoryId, int iid, string notes)
    {
        var user = await _userService.GetByIdAsync(UserId);
        return ToJson(await _reviewService.RequestChangesAsync(user, repositoryId, iid, notes));
    }

    [HttpPost]
    public async Task<JsonResponseModel> Merge(int repositoryId, int iid, string? sha, bool squash = false, bool removeSourceBranch = false)
    {
        var user = await _userService.GetByIdAsync(UserId);
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.MergeMergeRequests.SystemName, user))
            return ToJson(ReviewActionResult.Fail("You do not have the 'Merge Merge Requests' permission."));

        return ToJson(await _reviewService.MergeAsync(user, repositoryId, iid, sha, squash, removeSourceBranch));
    }

    [HttpPost]
    public async Task<JsonResponseModel> Rebase(int repositoryId, int iid)
    {
        var user = await _userService.GetByIdAsync(UserId);
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.MergeMergeRequests.SystemName, user))
            return ToJson(ReviewActionResult.Fail("You do not have the 'Merge Merge Requests' permission."));

        return ToJson(await _reviewService.RebaseAsync(user, repositoryId, iid));
    }

    #endregion

    #region Utilities

    private static JsonResponseModel ToJson(ReviewActionResult result) => new()
    {
        Status = result.Success ? HttpStatusCode.Success : HttpStatusCode.ValidationError,
        Message = result.Message
    };

    #endregion
}
