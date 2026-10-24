using Microsoft.AspNetCore.Mvc;
using Taskist.Core.Enums;
using Taskist.Service.Localization;
using Taskist.Service.Masters;
using Taskist.Service.Messages;
using Taskist.Service.Security;
using Taskist.Web.Controllers.Common;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Helpers.Mapping;
using Taskist.Web.Models.Common;
using Taskist.Web.Models.Datatable;
using Taskist.Web.Models.Masters;

namespace Taskist.Web.Controllers.Masters;

[CheckPermission(nameof(PermissionProvider.ManageEmailAccount))]
public class EmailAccountController : BaseController
{
	#region Fields

	private readonly IEmailAccountService _emailAccountService;
	private readonly IEmailTemplateService _emailTemplateService;
	private readonly IEmailService _emailService;
	protected readonly ILocalizationService _localizationService;
	private readonly MastersMapper _mapper;

	#endregion

	#region Ctor

	public EmailAccountController(IEmailAccountService emailAccountService,
		IEmailTemplateService emailTemplateService,
		IEmailService emailService,
		ILocalizationService localizationService,
		MastersMapper mapper)
	{
		_emailAccountService = emailAccountService;
		_emailTemplateService = emailTemplateService;
		_emailService = emailService;
		_localizationService = localizationService;
		_mapper = mapper;
	}

	#endregion

	#region Action

	public async Task<IActionResult> Index()
	{
		return View();
	}

	public async Task<IActionResult> Create()
	{
		var model = new EmailAccountModel();

		return PartialView(model);
	}

	[HttpPost]
	public async Task<IActionResult> Create(EmailAccountModel model)
	{
		if (ModelState.IsValid)
		{
			var entity = _mapper.ToEntity(model);

			await _emailAccountService.InsertAsync(entity);

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
		var entity = await _emailAccountService.GetByIdAsync(id);
		if (entity == null)
			return RedirectToAction("Index");

		var model = _mapper.ToModel(entity);
		model.Password = string.Empty;

		return PartialView(model);
	}

	[HttpPost]
	public async Task<IActionResult> Edit(EmailAccountModel model)
	{
		if (ModelState.IsValid)
		{
			var entity = await _emailAccountService.GetByIdAsync(model.Id);
			var currentPassword = entity.Password;
			_mapper.Update(model, entity);

			if (string.IsNullOrEmpty(model.Password))
				entity.Password = currentPassword;

			await _emailAccountService.UpdateAsync(entity);

			return Json(new JsonResponseModel
			{
				Status = HttpStatusCode.Success,
				Message = await _localizationService.GetResourceAsync("Message.UpdateSuccess")
			});
		}

		return Json(new JsonResponseModel
		{
			Status = ModelState.IsValid ? HttpStatusCode.InternalServerError : HttpStatusCode.ValidationError,
			Message = await _localizationService.GetResourceAsync("Error.Failed"),
			Errors = ModelState.AllErrors()
		});
	}

	public async Task<IActionResult> Delete(int id)
	{
		var entity = await _emailAccountService.GetByIdAsync(id);
		if (entity == null)
			return Json(new JsonResponseModel
			{
				Status = HttpStatusCode.NoData,
				Message = await _localizationService.GetResourceAsync("FormNoData.Description")
			});

		var emailTemplates = await _emailTemplateService.GetByEmailAccountIdAsync(id);
		var model = new EmailAccountModel
		{
			Id = id,
			EmailTemplates = emailTemplates.Select(x => _mapper.ToModel(x)).ToList()
		};

		return PartialView(model);
	}

	[HttpPost, ActionName("Delete")]
	public async Task<IActionResult> DeleteConfirmed(int id)
	{
		if (ModelState.IsValid)
		{
			var entity = await _emailAccountService.GetByIdAsync(id);

			await _emailAccountService.DeleteAsync(entity);

			return Json(new JsonResponseModel
			{
				Status = HttpStatusCode.Success,
				Message = await _localizationService.GetResourceAsync("Message.DeleteSuccess")
			});
		}

		return Json(new JsonResponseModel
		{
			Status = ModelState.IsValid ? HttpStatusCode.InternalServerError : HttpStatusCode.ValidationError,
			Message = await _localizationService.GetResourceAsync("Error.Failed"),
			Errors = ModelState.AllErrors()
		});
	}

	public async Task<IActionResult> SendTest(int id)
	{
		var entity = await _emailAccountService.GetByIdAsync(id);
		if (entity == null)
			return RedirectToAction("Index");

		var model = new TestEmailModel { Id = id };

		return PartialView(model);
	}

	[HttpPost]
	public async Task<IActionResult> SendTest(TestEmailModel model)
	{
		if (!ModelState.IsValid)
			return Json(new JsonResponseModel
			{
				Status = HttpStatusCode.ValidationError,
				Message = await _localizationService.GetResourceAsync("Error.Failed"),
				Errors = ModelState.AllErrors()
			});

		var entity = await _emailAccountService.GetByIdAsync(model.Id);
		if (entity == null)
			return Json(new JsonResponseModel
			{
				Status = HttpStatusCode.NoData,
				Message = await _localizationService.GetResourceAsync("FormNoData.Description")
			});

		try
		{
			var subject = await _localizationService.GetResourceAsync("TestEmail.Subject");
			var body = string.Format(await _localizationService.GetResourceAsync("TestEmail.Body"),
				entity.Name, entity.Host, entity.Port);

			await _emailService.SendEmailAsync(entity, subject, body, model.ToEmail, model.ToEmail);
		}
		catch (Exception ex)
		{
			return Json(new JsonResponseModel
			{
				Status = HttpStatusCode.InternalServerError,
				Message = string.Format(await _localizationService.GetResourceAsync("TestEmail.FailedMsg"), ex.Message)
			});
		}

		return Json(new JsonResponseModel
		{
			Status = HttpStatusCode.Success,
			Message = await _localizationService.GetResourceAsync("TestEmail.SuccessMsg")
		});
	}

	#endregion

	#region Data

	[HttpPost]
	public async Task<IActionResult> DataRead(DataTableRequest request)
	{
		var data = await _emailAccountService.GetPagedListAsync(request.SearchValue, request.Start,
			request.Length, request.SortColumn, request.SortDirection);

		return Json(new
		{
			request.Draw,
			data = data.Select(x => _mapper.ToModel(x)),
			recordsFiltered = data.TotalCount,
			recordsTotal = data.TotalCount
		});
	}

	#endregion
}
