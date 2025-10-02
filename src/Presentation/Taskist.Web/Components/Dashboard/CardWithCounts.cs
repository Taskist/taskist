using Taskist.Core.Common;
using Taskist.Service.WorkItems;
using Microsoft.AspNetCore.Mvc;
using Taskist.Web.Models.Dashboard;

namespace Taskist.Web.Components.Dashboard;

public class CardWithCounts : ViewComponent
{
    #region Fields

    protected readonly IWorkContext _workContext;
    protected readonly IBacklogItemService _backlogService;

    #endregion

    #region Ctor

    public CardWithCounts(IWorkContext workContext,
        IBacklogItemService backlogService)
    {
        _workContext = workContext;
        _backlogService = backlogService;
    }

    #endregion

    #region Methods

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var user = await _workContext.GetCurrentUserAsync();
        var tasks = await _backlogService.GetTaskCountAsync(user.Id);

        var model = new List<CardModel>();

        foreach (var m in tasks)
        {
            var cardModel = new CardModel
            {
                Name = m.Key,
                Count = m.Value,
                FilterMode = m.Key switch
                {
                    "My Tasks" => 1,
                    "Reopened Tasks" => 2,
                    "Closed Tasks" => 3,
                    _ => 0,
                }
            };
            model.Add(cardModel);
        }

        return View(model);
    }

    #endregion
}