using Newtonsoft.Json;
using Taskist.Core.Common;
using Taskist.Core.Domain.Settings;
using Taskist.Core.Enums;
using Taskist.Service.Masters;

namespace Taskist.Web.Helpers.Reports;

public class ReportLayoutService : IReportLayoutService
{
    #region Fields

    private const string AttributePrefix = "ReportLayout.";

    protected readonly IGenericAttributeService _genericAttributeService;
    protected readonly IWorkContext _workContext;
    protected readonly ReportExportSettings _reportExportSettings;

    #endregion

    #region Ctor

    public ReportLayoutService(IGenericAttributeService genericAttributeService,
        IWorkContext workContext,
        ReportExportSettings reportExportSettings)
    {
        _genericAttributeService = genericAttributeService;
        _workContext = workContext;
        _reportExportSettings = reportExportSettings;
    }

    #endregion

    #region Methods

    public List<ReportColumn> GetDefaultLayout(ReportExportKey key)
    {
        return ReportLayoutResolver.GetEditableLayout(_reportExportSettings.LayoutsJson, key);
    }

    public async Task<List<ReportColumn>> GetUserLayoutAsync(ReportExportKey key)
    {
        var defaults = GetDefaultLayout(key);
        var saved = ReportLayoutResolver.ParseUserLayout(await GetAttributeAsync(key));

        return saved.Count == 0 ? defaults : ReportLayoutResolver.ApplyUserLayout(defaults, saved);
    }

    public async Task<List<ReportColumn>> GetExportLayoutAsync(ReportExportKey key)
    {
        return (await GetUserLayoutAsync(key)).Where(c => c.Included).ToList();
    }

    public async Task<bool> HasCustomLayoutAsync(ReportExportKey key)
    {
        return ReportLayoutResolver.ParseUserLayout(await GetAttributeAsync(key)).Count > 0;
    }

    public async Task SaveUserLayoutAsync(ReportExportKey key, IEnumerable<ReportColumn> columns)
    {
        var defaults = GetDefaultLayout(key);
        var layout = ReportLayoutResolver.ApplyUserLayout(defaults, columns);

        var value = ReportLayoutResolver.SameLayout(layout, defaults)
            ? string.Empty
            : JsonConvert.SerializeObject(layout.Select(c => new { c.Key, c.Included }));

        await SaveAttributeAsync(key, value);
    }

    public async Task ResetUserLayoutAsync(ReportExportKey key)
    {
        await SaveAttributeAsync(key, string.Empty);
    }

    #endregion

    #region Utilities

    private async Task<string> GetAttributeAsync(ReportExportKey key)
    {
        var user = await _workContext.GetCurrentUserAsync();
        return user == null ? null : await _genericAttributeService.GetAttributeAsync<string>(user, AttributePrefix + key);
    }

    private async Task SaveAttributeAsync(ReportExportKey key, string value)
    {
        var user = await _workContext.GetCurrentUserAsync();
        if (user != null)
            await _genericAttributeService.SaveAttributeAsync(user, AttributePrefix + key, value);
    }

    #endregion
}
