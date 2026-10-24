using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Web.Helpers.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Taskist.Web.Controllers.Common;

[Authorize]
[ValidateUserId]
[AutoValidateAntiforgeryToken]
[SaveIpAddress]
[SaveLastActivity]
[SaveLastVisitedPage]
[ValidateIpAddress]
public class BaseController : Controller
{
	#region Actions

	protected IActionResult AccessDenied()
	{
		return RedirectToAction("AccessDenied", "Common");
	}

	protected IActionResult AccessDeniedPartial()
	{
		return RedirectToAction("AccessDeniedPartial", "Common");
	}

	protected IActionResult ProjectAccessDenied(string message)
	{
		return RedirectToAction("ProjectAccessDenied", "Common", new { message });
	}

	protected IActionResult AccessDeniedDataRead()
	{
		return Json(new
		{
			error = "Access denied!"
		});
	}

	protected IActionResult NoData()
	{
		return RedirectToAction("NoData", "Common");
	}

	protected IActionResult NoDataPartial()
	{
		return RedirectToAction("NoDataPartial", "Common");
	}

	protected IActionResult DocumentFile(Document document)
	{
		Response.Headers["X-Content-Type-Options"] = "nosniff";

		var imageType = UploadPolicy.ImageContentType(document.Extension);
		if (imageType != null)
			return File(document.FileData, imageType);

		var name = string.IsNullOrWhiteSpace(document.FileName) ? "attachment" : document.FileName;
		return File(document.FileData, "application/octet-stream", name + document.Extension);
	}

	protected IActionResult ImageFile(Document document)
	{
		var imageType = UploadPolicy.ImageContentType(document.Extension);
		if (imageType == null || document.FileData == null || document.FileData.Length == 0)
			return NotFound();

		Response.Headers["X-Content-Type-Options"] = "nosniff";
		return File(document.FileData, imageType);
	}

	#endregion Actions

	#region Properties

	protected int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

	protected string? UserEmail => User.FindFirstValue(ClaimTypes.Email);

	protected string? UserName => User.FindFirstValue(ClaimTypes.Name);

	#endregion
}