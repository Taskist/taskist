using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.RegularExpressions;
using Taskist.Core.Common;
using Taskist.Core.Domain.Settings;
using Taskist.Core.Enums;
using Taskist.Core.Extensions;
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

[CheckPermission(nameof(PermissionProvider.ManageEmailTemplate))]
public class EmailTemplateController : BaseController
{
    #region Fields

    private readonly IEmailTemplateService _emailTemplateService;
    private readonly IEmailAccountService _emailAccountService;
    protected readonly ILocalizationService _localizationService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IAppLinkBuilder _linkBuilder;
    private readonly GeneralSettings _generalSettings;
    private readonly MastersMapper _mapper;

    #endregion

    #region Ctor

    public EmailTemplateController(IEmailTemplateService emailTemplateService,
            IEmailAccountService emailAccountService,
            ILocalizationService localizationService,
            IWebHostEnvironment webHostEnvironment,
            IAppLinkBuilder linkBuilder,
            GeneralSettings generalSettings,
            MastersMapper mapper)
    {
        _emailTemplateService = emailTemplateService;
        _emailAccountService = emailAccountService;
        _localizationService = localizationService;
        _webHostEnvironment = webHostEnvironment;
        _linkBuilder = linkBuilder;
        _generalSettings = generalSettings;
        _mapper = mapper;
    }

    #endregion

    #region Action

    public async Task<IActionResult> Index()
    {
        return View();
    }

    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _emailTemplateService.GetByIdAsync(id);
        if (entity == null)
            return RedirectToAction("Index");

        var model = _mapper.ToModel(entity);
        await InitModel(model);

        return PartialView(model);
    }

    [HttpPost]
    public IActionResult Preview(string name, string emailBody)
    {
        return RenderPreview(name, emailBody ?? string.Empty);
    }

    [HttpGet]
    public async Task<IActionResult> Preview(int id)
    {
        var entity = await _emailTemplateService.GetByIdAsync(id);
        if (entity == null)
            return Content(string.Empty, "text/html");

        return RenderPreview(entity.Name, entity.EmailBody ?? string.Empty);
    }

    private IActionResult RenderPreview(string name, string emailBody)
    {
        var sampleBody = ApplySampleTokens(name, emailBody);
        var html = EmailLayoutBuilder.Wrap(sampleBody, _linkBuilder.Build("images/brand/logo.png"), _generalSettings.CompanyName);

        return Content(html, "text/html");
    }

    private string ApplySampleTokens(string name, string body)
    {
        var appUrl = _linkBuilder.Build(string.Empty);

        body = body
            .Replace("%displayname%", "Alex Morgan")
            .Replace("%display_name%", "Alex Morgan")
            .Replace("%url%", _linkBuilder.Build("setpassword"))
            .Replace("%button%", appUrl)
            .Replace("%body%", "<p>This is a sample notification message.</p><p><a href=\"" + appUrl + "\">Open in Taskist</a></p>")
            .Replace("%year%", DateTime.Today.Year.ToString())
            .Replace("%company%", _generalSettings.CompanyNameOrDefault());

        var tokens = new Dictionary<string, string>
        {
            { "url", _linkBuilder.Build("backlog/edit/1024") },
            { "task_id", "1024" },
            { "display_name", "Alex Morgan" },
            { "task_type", "Bug" },
            { "title", "Login page returns 500 on submit" },
            { "project_name", "Acme Portal" },
            { "module_name", "Accounts" },
            { "sub_module_name", "Login" },
            { "created_by", "Priya Nair" },
            { "updated_by", "Priya Nair" },
            { "severity_name", "High" },
            { "new_status", "In-Progress" },
            { "due_date", DateTime.Today.AddDays(5).ToString("dd-MMM-yyyy") }
        };

        return Regex.Replace(body, @"\{(\w+)\}", m => tokens.TryGetValue(m.Groups[1].Value, out var v) ? v : m.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EmailTemplateModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = await _emailTemplateService.GetByIdAsync(model.Id);
            _mapper.Update(model, entity);

            await _emailTemplateService.UpdateAsync(entity);

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

    public async Task<IActionResult> Reset()
    {
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
        var defaultEmailAccount = await _emailAccountService.GetFirstActiveAsync();

        if (defaultEmailAccount == null)
            return Json(new JsonResponseModel
            {
                Status = HttpStatusCode.ValidationError,
                Message = await _localizationService.GetResourceAsync("EmailTemplateModel.DefaultAccount.NotExistMsg")
            });

        foreach (var val in Enum.GetValues(typeof(EmailTemplateType)))
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

            if (string.IsNullOrEmpty(template))
                continue;

            var entity = await _emailTemplateService.GetByNameAsync(val.ToString());
            if (entity != null)
            {
                entity.EmailBody = template;
                await _emailTemplateService.UpdateAsync(entity);

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

                var newEntity = _mapper.ToEntity(templateModel);

                await _emailTemplateService.InsertAsync(newEntity);

            }
        }

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCode.Success,
            Message = await _localizationService.GetResourceAsync("Message.UpdateSuccess")
        });
    }

    #endregion

    #region Data

    [HttpPost]
    public async Task<IActionResult> DataRead(DataTableRequest request)
    {
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
