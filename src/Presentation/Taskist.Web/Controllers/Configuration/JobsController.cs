using Microsoft.AspNetCore.Mvc;
using Taskist.Service.Security;
using Taskist.Web.Controllers.Common;
using Taskist.Web.Helpers.Attributes;

namespace Taskist.Web.Controllers.Configuration;

[CheckPermission(nameof(PermissionProvider.ManageJobs))]
public class JobsController : BaseController
{
    public IActionResult Index()
    {
        return Redirect("/hangfire");
    }
}
