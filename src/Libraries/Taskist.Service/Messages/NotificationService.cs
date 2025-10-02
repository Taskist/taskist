using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using Taskist.Service.Common;
using Taskist.Service.Logging;
using Taskist.Core.Common;

namespace Taskist.Service.Messages;

public class NotificationService : INotificationService
{
    #region Fields

    protected readonly IHttpContextAccessor _httpContextAccessor;
    protected readonly ILogService _logService;
    protected readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;
    protected readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public NotificationService(IHttpContextAccessor httpContextAccessor,
        ILogService logService, 
        ITempDataDictionaryFactory tempDataDictionaryFactory,
        IWorkContext workContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _logService = logService;
        _tempDataDictionaryFactory = tempDataDictionaryFactory;
        _workContext = workContext;
    }

    #endregion

    #region Utilities

    protected void PrepareTempData(NotifyType type, string message, bool encode = true)
    {
        var context = _httpContextAccessor.HttpContext;
        var tempData = _tempDataDictionaryFactory.GetTempData(context);

        var messages = tempData.ContainsKey(ServiceConstant.NotificationListKey)
            ? JsonConvert.DeserializeObject<IList<NotifyData>>(tempData[ServiceConstant.NotificationListKey].ToString())
            : new List<NotifyData>();

        messages.Add(new NotifyData
        {
            Message = message,
            Type = type,
            Encode = encode
        });

        tempData[ServiceConstant.NotificationListKey] = JsonConvert.SerializeObject(messages);
    }

    protected async Task LogExceptionAsync(Exception exception)
    {
        if (exception == null)
            return;
        var customer = await _workContext.GetCurrentUserAsync();
        await _logService.ErrorAsync(exception.Message, exception, customer);
    }

    #endregion

    #region Methods

    public void Notification(NotifyType type, string message, bool encode = true)
    {
        PrepareTempData(type, message, encode);
    }

    public void SuccessNotification(string message, bool encode = true)
    {
        PrepareTempData(NotifyType.Success, message, encode);
    }

    public void WarningNotification(string message, bool encode = true)
    {
        PrepareTempData(NotifyType.Warning, message, encode);
    }

    public void ErrorNotification(string message, bool encode = true)
    {
        PrepareTempData(NotifyType.Error, message, encode);
    }

    public async Task ErrorNotificationAsync(Exception exception, bool logException = true)
    {
        if (exception == null)
            return;

        if (logException)
            await LogExceptionAsync(exception);

        ErrorNotification(exception.Message);
    }

    #endregion
}
