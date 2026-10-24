using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Taskist.Core.Enums;
using Taskist.Service.Localization;
using Taskist.Web.Controllers.Common;
using Taskist.Web.Helpers.Reports;
using Taskist.Web.Models.Common;
using Taskist.Web.Models.Configuration;
using Taskist.Web.Models.Reports;

namespace Taskist.Web.Controllers.Reports;

public class ReportLayoutController : BaseController
{
    #region Fields

    protected readonly IReportLayoutService _reportLayoutService;
    protected readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public ReportLayoutController(IReportLayoutService reportLayoutService,
        ILocalizationService localizationService)
    {
        _reportLayoutService = reportLayoutService;
        _localizationService = localizationService;
    }

    #endregion

    #region Actions

    [HttpGet]
    public async Task<IActionResult> Editor(ReportExportKey key)
    {
        if (!Enum.IsDefined(key))
            return BadRequest();

        var model = new ReportLayoutEditorModel
        {
            IsCustom = await _reportLayoutService.HasCustomLayoutAsync(key),
            Layout = new ReportLayoutModel
            {
                Key = key.ToString(),
                Name = ReportColumnRegistry.GetName(key),
                Columns = (await _reportLayoutService.GetUserLayoutAsync(key))
                    .Select(c => new ReportColumnItemModel { Key = c.Key, Title = c.Title, Included = c.Included })
                    .ToList()
            }
        };

        return PartialView(model);
    }

    [HttpPost]
    public async Task<JsonResponseModel> Save(ReportExportKey key, string columns)
    {
        if (!Enum.IsDefined(key))
            return new JsonResponseModel { Status = HttpStatusCode.ValidationError };

        List<ReportColumn> parsed;
        try
        {
            parsed = JsonConvert.DeserializeObject<List<ReportColumn>>(columns ?? "[]") ?? [];
        }
        catch (JsonException)
        {
            return new JsonResponseModel
            {
                Status = HttpStatusCode.ValidationError,
                Message = await _localizationService.GetResourceAsync("ReportSettings.InvalidJson")
            };
        }

        await _reportLayoutService.SaveUserLayoutAsync(key, parsed);

        return new JsonResponseModel
        {
            Status = HttpStatusCode.Success,
            Message = await _localizationService.GetResourceAsync("ReportLayout.Saved")
        };
    }

    [HttpPost]
    public async Task<JsonResponseModel> Reset(ReportExportKey key)
    {
        if (!Enum.IsDefined(key))
            return new JsonResponseModel { Status = HttpStatusCode.ValidationError };

        await _reportLayoutService.ResetUserLayoutAsync(key);

        return new JsonResponseModel
        {
            Status = HttpStatusCode.Success,
            Message = await _localizationService.GetResourceAsync("ReportLayout.ResetDone")
        };
    }

    #endregion
}
