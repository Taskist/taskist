using Microsoft.AspNetCore.Mvc;
using Taskist.Service.Masters;
using Taskist.Service.WorkItems;
using Taskist.Web.Helpers.Mapping;
using Taskist.Web.Models.Masters;

namespace Taskist.Web.Components.WorkItems;

public class CustomFields : ViewComponent
{
	#region Fields

	protected readonly ICustomFieldService _customFieldService;
	protected readonly IBacklogService _backlogService;
	protected readonly MastersMapper _mastersMapper;
	protected readonly WorkItemsMapper _workItemsMapper;

	#endregion

	#region Ctor

	public CustomFields(ICustomFieldService customFieldService,
		IBacklogService backlogService,
		MastersMapper mastersMapper,
		WorkItemsMapper workItemsMapper)
	{
		_customFieldService = customFieldService;
		_backlogService = backlogService;
		_mastersMapper = mastersMapper;
		_workItemsMapper = workItemsMapper;
	}

	#endregion

	#region Methods

	public async Task<IViewComponentResult> InvokeAsync(int backLogId, int placement, int projectId)
	{
		var customFields = await _customFieldService.GetAllAsync(projectId);
		var customFieldValues = await _backlogService.GetAllCustomFieldValuesAsync(backLogId);

		var model = new CustomFieldsViewModel()
		{
			Index = placement > 1 ? customFields.Count(x => x.Placement == 1) : 0,
			BacklogId = backLogId,
			CustomFields = customFields.Where(x => x.Placement == placement).Select(x => _mastersMapper.ToRenderModel(x)).ToList(),
			Values = customFieldValues.Select(x => _workItemsMapper.ToModel(x)).ToList()
		};

		return View(model);
	}

	#endregion
}
