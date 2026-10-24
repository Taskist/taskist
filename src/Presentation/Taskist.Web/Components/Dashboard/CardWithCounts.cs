using Taskist.Core.Common;
using Taskist.Service.WorkItems;
using Taskist.Web.Models.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace Taskist.Web.Components.Dashboard;

public class CardWithCounts : ViewComponent
{
    #region Fields

    protected readonly IWorkContext _workContext;
    protected readonly IBacklogService _backlogService;

    #endregion

    #region Ctor

    public CardWithCounts(IWorkContext workContext,
        IBacklogService backlogService)
    {
        _workContext = workContext;
        _backlogService = backlogService;
    }

    #endregion

    #region Methods

    public async Task<IViewComponentResult> InvokeAsync(int[] projectIds, int selectedProjectId = 0)
    {
        var userId = _workContext.GetCurrentUserId() ?? 0;
        var tasks = await _backlogService.GetTaskCountAsync(userId, projectIds);

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
                    _ => 5,
                }
            };
            model.Add(cardModel);
        }

        return View(new CardWithCountsModel { Cards = model, SelectedProjectId = selectedProjectId });
    }

    #endregion
}