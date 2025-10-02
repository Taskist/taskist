using Microsoft.AspNetCore.Mvc;

namespace Taskist.Web.Controllers.Common;

public class HomeController : BaseController
{
    public async Task<IActionResult> Index()
    {
        return View();
    }
}