using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Service.Localization;
using Taskist.Service.Logging;
using Taskist.Service.Masters;
using Taskist.Service.Security;
using Taskist.Web.Controllers.Common;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Common;
using Taskist.Web.Models.Datatable;
using Taskist.Web.Models.Masters;

namespace Taskist.Web.Controllers.Masters;

public class EmailAccountController : BaseController
{
    #region Fields

    private readonly IEmailAccountService _emailAccountService;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly IPermissionService _permissionService;
    protected readonly ILocalizationService _localizationService;
    private readonly IUserActivityService _userActivityService;
    protected readonly IWorkContext _workContext;
    private readonly IMapper _mapper;

    #endregion

    #region Ctor

    public EmailAccountController(IEmailAccountService emailAccountService,
        IEmailTemplateService emailTemplateService,
        IPermissionService permissionService,
        ILocalizationService localizationService,
        IUserActivityService userActivityService,
        IWorkContext workContext,
        IMapper mapper)
    {
        _emailAccountService = emailAccountService;
        _emailTemplateService = emailTemplateService;
        _permissionService = permissionService;
        _localizationService = localizationService;
        _userActivityService = userActivityService;
        _workContext = workContext;
        _mapper = mapper;
    }

    #endregion

    #region Action

    [CheckPermission(PermissionProvider.Configuration.MANAGE_EMAIL_ACCOUNTS)]
    public async Task<IActionResult> Index()
    {

        return View();
    }

    [CheckPermission(PermissionProvider.Configuration.MANAGE_EMAIL_ACCOUNTS)]
    public async Task<IActionResult> Create()
    {
        var model = new EmailAccountModel();

        return PartialView(model);
    }

    [HttpPost]
    [CheckPermission(PermissionProvider.Configuration.MANAGE_EMAIL_ACCOUNTS)]
    public async Task<IActionResult> Create(EmailAccountModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = _mapper.Map<EmailAccount>(model);
            var user = await _workContext.GetCurrentUserAsync();

            await _emailAccountService.InsertAsync(entity);

            await _userActivityService.InsertAsync("EmailAccount", string.Format(await _localizationService.GetResourceAsync("Log.RecordCreated"), entity.Name), entity);

            return Json(new JsonResponseModel
            {
                Status = HttpStatusCodeEnum.Success,
                Message = await _localizationService.GetResourceAsync("Message.SaveSuccess")
            });
        }

        return Json(new JsonResponseModel
        {
            Status = ModelState.IsValid ? HttpStatusCodeEnum.InternalServerError : HttpStatusCodeEnum.ValidationError,
            Message = await _localizationService.GetResourceAsync("Error.Failed"),
            Errors = ModelState.AllErrors()
        });
    }

    [CheckPermission(PermissionProvider.Configuration.MANAGE_EMAIL_ACCOUNTS)]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _emailAccountService.GetByIdAsync(id);
        if (entity == null)
            return RedirectToAction("Index");

        var model = _mapper.Map<EmailAccountModel>(entity);

        return PartialView(model);
    }

    [HttpPost]
    [CheckPermission(PermissionProvider.Configuration.MANAGE_EMAIL_ACCOUNTS)]
    public async Task<IActionResult> Edit(EmailAccountModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = await _emailAccountService.GetByIdAsync(model.Id);
            entity = _mapper.Map(model, entity);

            await _emailAccountService.UpdateAsync(entity);

            await _userActivityService.InsertAsync("EmailAccount", string.Format(await _localizationService.GetResourceAsync("Log.RecordUpdated"), entity.Name), entity);

            return Json(new JsonResponseModel
            {
                Status = HttpStatusCodeEnum.Success,
                Message = await _localizationService.GetResourceAsync("Message.UpdateSuccess")
            });
        }

        return Json(new JsonResponseModel
        {
            Status = ModelState.IsValid ? HttpStatusCodeEnum.InternalServerError : HttpStatusCodeEnum.ValidationError,
            Message = await _localizationService.GetResourceAsync("Error.Failed"),
            Errors = ModelState.AllErrors()
        });
    }

    [CheckPermission(PermissionProvider.Configuration.MANAGE_EMAIL_ACCOUNTS)]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _emailAccountService.GetByIdAsync(id);
        if (entity == null)
            return Json(new JsonResponseModel
            {
                Status = HttpStatusCodeEnum.NoData,
                Message = await _localizationService.GetResourceAsync("FormNoData.Description")
            });

        var emailTemplates = await _emailTemplateService.GetByEmailAccountIdAsync(id);
        var model = new EmailAccountModel
        {
            Id = id,
            EmailTemplates = _mapper.Map<List<EmailTemplateModel>>(emailTemplates)
        };

        return PartialView(model);
    }

    [HttpPost, ActionName("Delete")]
    [CheckPermission(PermissionProvider.Configuration.MANAGE_EMAIL_ACCOUNTS)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (ModelState.IsValid)
        {
            var entity = await _emailAccountService.GetByIdAsync(id);

            await _emailAccountService.DeleteAsync(entity);

            await _userActivityService.InsertAsync("EmailAccount", string.Format(await _localizationService.GetResourceAsync("Log.RecordDeleted"), entity.Name), entity);

            return Json(new JsonResponseModel
            {
                Status = HttpStatusCodeEnum.Success,
                Message = await _localizationService.GetResourceAsync("Message.DeleteSuccess")
            });
        }

        return Json(new JsonResponseModel
        {
            Status = ModelState.IsValid ? HttpStatusCodeEnum.InternalServerError : HttpStatusCodeEnum.ValidationError,
            Message = await _localizationService.GetResourceAsync("Error.Failed"),
            Errors = ModelState.AllErrors()
        });
    }

    #endregion

    #region Data

    [HttpPost]
    [CheckPermission(PermissionProvider.Configuration.MANAGE_EMAIL_ACCOUNTS)]
    public async Task<IActionResult> DataRead(DataTableRequest request)
    {
        var data = await _emailAccountService.GetPagedListAsync(request.SearchValue, request.Start,
            request.Length, request.SortColumn, request.SortDirection);

        return Json(new
        {
            request.Draw,
            data = data.Select(x => _mapper.Map<EmailAccountModel>(x)),
            recordsFiltered = data.TotalCount,
            recordsTotal = data.TotalCount
        });
    }

    #endregion
}