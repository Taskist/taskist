using Taskist.Core.Common;
using Taskist.Core.Extensions;
using Taskist.Service.Logging;
using Taskist.Service.Masters;
using Taskist.Service.Security;
using Taskist.Web.Models.Common;
using Microsoft.AspNetCore.Mvc;

namespace Taskist.Web.Controllers.Common;

public class CommonController : BaseController
{
	#region Fields

	protected readonly ISystemService _systemService;
	protected readonly IGenericAttributeService _genericAttributeService;
	protected readonly ILogService _logService;

	#endregion

	#region Ctor

	public CommonController(ISystemService systemService,
		IGenericAttributeService genericAttributeService,
		ILogService logService)
	{
		_systemService = systemService;
		_genericAttributeService = genericAttributeService;
		_logService = logService;
	}

	#endregion

	#region Actions Related To Error

	public ActionResult AccessDenied()
	{
		return View();
	}

	public ActionResult AccessDeniedPartial()
	{
		return PartialView();
	}

	public ActionResult NoData()
	{
		return View();
	}

	public ActionResult NoDataPartial()
	{
		return PartialView();
	}

	public ActionResult ProjectAccessDenied(string message)
	{
		return View(model: message);
	}

	public IActionResult PageNotFound()
	{
		Response.StatusCode = 404;
		Response.ContentType = "text/html";

		return View();
	}

	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	public async Task<IActionResult> Error(int? id)
	{
		var model = new ErrorModel("ApplicationError", null);

		if (id > 0)
		{
			var log = await _logService.GetByIdAsync(id.ToInt());
			model.Message = log.FullMessage;
		}
		return View(model);
	}

	#endregion

	#region Actions

	[HttpGet("/ping")]
	public IActionResult Ping() => Ok("pong");

	public async Task<IActionResult> ResetCache()
	{
		await _systemService.ResetCacheAsync();

		var refererUrl = Request.Headers["Referer"].ToString();
		if (!string.IsNullOrEmpty(refererUrl))
		{
			return Redirect(refererUrl);
		}

		return RedirectToAction("Index", "Home");
	}

	#endregion
}