using Microsoft.AspNetCore.Mvc;
using Taskist.Service.Security;
using Taskist.Service.Users;
using Taskist.Service.WorkItems;
using Taskist.Web.Controllers.Common;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.WorkItems;

namespace Taskist.Web.Controllers.WorkItems;

[CheckPermission(nameof(PermissionProvider.ManageBacklog))]
public class MyCodeController : BaseController
{
    #region Fields

    protected readonly IMyCodeService _myCodeService;
    protected readonly IUserService _userService;
    protected readonly IPermissionService _permissionService;

    #endregion

    #region Ctor

    public MyCodeController(IMyCodeService myCodeService,
        IUserService userService,
        IPermissionService permissionService)
    {
        _myCodeService = myCodeService;
        _userService = userService;
        _permissionService = permissionService;
    }

    #endregion

    #region Actions

    [HttpGet]
    public async Task<IActionResult> Index(bool refresh = false)
    {
        var user = await _userService.GetByIdAsync(UserId);
        if (user == null)
            return NoDataPartial();

        return PartialView("_MyCode", new MyCodeViewModel
        {
            Code = await _myCodeService.GetAsync(user, refresh),
            CanReview = await _permissionService.AuthorizeAsync(PermissionProvider.ReviewMergeRequests)
        });
    }

    #endregion
}
