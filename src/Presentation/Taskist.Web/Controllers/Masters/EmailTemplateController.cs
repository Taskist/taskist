using AutoMapper;
using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Service.Localization;
using Taskist.Service.Logging;
using Taskist.Service.Masters;
using Taskist.Service.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Taskist.Web.Controllers.Common;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Common;
using Taskist.Web.Models.Datatable;
using Taskist.Web.Models.Masters;

namespace Taskist.Web.Controllers.Masters;

public class EmailTemplateController : BaseController
{
    #region Fields

    private readonly IEmailTemplateService _emailTemplateService;
    private readonly IEmailAccountService _emailAccountService;
    private readonly IPermissionService _permissionService;
    protected readonly ILocalizationService _localizationService;
    private readonly IUserActivityService _userActivityService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    protected readonly IWorkContext _workContext;
    private readonly IMapper _mapper;

    #endregion

    #region Ctor

    public EmailTemplateController(IEmailTemplateService emailTemplateService,
        IEmailAccountService emailAccountService,
        IPermissionService permissionService,
        ILocalizationService localizationService,
        IUserActivityService userActivityService,
        IWebHostEnvironment webHostEnvironment,
        IWorkContext workContext,
        IMapper mapper)
    {
        _emailTemplateService = emailTemplateService;
        _emailAccountService = emailAccountService;
        _permissionService = permissionService;
        _localizationService = localizationService;
        _userActivityService = userActivityService;
        _webHostEnvironment = webHostEnvironment;
        _workContext = workContext;
        _mapper = mapper;
    }

    #endregion

    #region Action

    public async Task<IActionResult> Index()
    {
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.ManageEmailTemplate))
            return AccessDenied();

        return View();
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.ManageEmailTemplate))
            return AccessDeniedPartial();

        var entity = await _emailTemplateService.GetByIdAsync(id);
        if (entity == null)
            return RedirectToAction("Index");

        var model = _mapper.Map<EmailTemplateModel>(entity);
        await InitModel(model);

        return PartialView(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EmailTemplateModel model)
    {
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.ManageEmailTemplate))
            return AccessDeniedPartial();

        if (ModelState.IsValid)
        {
            var entity = await _emailTemplateService.GetByIdAsync(model.Id);
            entity = _mapper.Map(model, entity);

            await _emailTemplateService.UpdateAsync(entity);

            await _userActivityService.InsertAsync("EmailTemplate", string.Format(await _localizationService.GetResourceAsync("Log.RecordUpdated"), entity.Name), entity);

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

    public async Task<IActionResult> Reset()
    {
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.ManageEmailTemplate))
            return AccessDeniedPartial();

        var model = new PageModel
        {
            Valid = true
        };

        var defaultEmailAccount = await _emailAccountService.GetFirstActiveAsync();

        if (defaultEmailAccount == null)
        {
            model.Valid = false;
            model.Message = await _localizationService.GetResourceAsync("EmailTemplateModel.DefaultAccount.NotExistMsg");
        }

        return PartialView(model);
    }

    [HttpPost]
    public async Task<IActionResult> Reset(IFormCollection formCollection)
    {
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.ManageEmailTemplate))
            return AccessDeniedPartial();

        //var user = _workContext.ActiveUser;
        var defaultEmailAccount = await _emailAccountService.GetFirstActiveAsync();

        if (defaultEmailAccount == null)
            return Json(new JsonResponseModel
            {
                Status = HttpStatusCodeEnum.ValidationError,
                Message = await _localizationService.GetResourceAsync("EmailTemplateModel.DefaultAccount.NotExistMsg")
            });

        foreach (var val in Enum.GetValues(typeof(EmailTemplateTypeEnum)))
        {
            if (!formCollection.Keys.Any(x => x == val.ToString()))
                continue;

            var defaultTemplatePath = Path.Combine(_webHostEnvironment.WebRootPath, "templates", "email", $"{val.ToString().ToLower()}.html");

            if (!System.IO.File.Exists(defaultTemplatePath))
                continue;

            string template = null;
            using (StreamReader sr = new StreamReader(defaultTemplatePath))
            {
                template = sr.ReadToEnd();
            }

            //Continue if template is empty
            if (string.IsNullOrEmpty(template))
                continue;

            var entity = await _emailTemplateService.GetByNameAsync(val.ToString());
            if (entity != null)
            {
                entity.EmailBody = template;
                await _emailTemplateService.UpdateAsync(entity);

                await _userActivityService.InsertAsync("EmailTemplate", string.Format(await _localizationService.GetResourceAsync("Log.RecordUpdated"), entity.Name), entity);
            }
            else
            {
                var templateModel = new EmailTemplateModel
                {
                    Name = val.ToString(),
                    EmailSubject = val.ToString(),
                    EmailBody = template,
                    Active = true,
                    EmailAccountId = defaultEmailAccount.Id
                };

                var newEntity = _mapper.Map<EmailTemplate>(templateModel);

                await _emailTemplateService.InsertAsync(newEntity);

                await _userActivityService.InsertAsync("EmailTemplate", string.Format(await _localizationService.GetResourceAsync("Log.RecordCreated"), newEntity.Name), newEntity);
            }
        }

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCodeEnum.Success,
            Message = await _localizationService.GetResourceAsync("Message.UpdateSuccess")
        });
    }

    #endregion

    #region Data

    [HttpPost]
    public async Task<IActionResult> DataRead(DataTableRequest request)
    {
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.ManageEmailTemplate))
            return AccessDeniedDataRead();

        var data = await _emailTemplateService.GetPagedListAsync(request.SearchValue, request.Start,
            request.Length, request.SortColumn, request.SortDirection);

        return Json(new
        {
            request.Draw,
            data = data.Select(x => new EmailTemplateGridModel
            {
                Id = x.Id,
                Name = x.Name,
                EmailSubject = x.EmailSubject,
                EmailAccount = x.EmailAccount.Name,
                Active = x.Active
            }),
            recordsFiltered = data.TotalCount,
            recordsTotal = data.TotalCount
        });
    }

    #endregion

    #region Helper

    private async Task InitModel(EmailTemplateModel model)
    {
        var emailAccounts = await _emailAccountService.GetAllActiveAsync();
        model.AvailableEmailAccounts = emailAccounts.Select(x => new SelectListItem
        {
            Value = x.Id.ToString(),
            Text = x.Name
        }).ToList();
    }

    #endregion
}