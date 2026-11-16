using Microsoft.AspNetCore.Mvc;
using Taskist.Core.Common;
using Taskist.Service.Localization;
using Taskist.Service.Masters;
using Taskist.Web.Helpers.Mapping;
using Taskist.Web.Models.Masters;

namespace Taskist.Web.Components.Common;

public class Menu : ViewComponent
{
	#region Fields

	private readonly IWorkContext _workContext;
	private readonly IMenuService _menuService;
	private readonly ILocalizationService _localizationService;
	private readonly MastersMapper _mastersMapper;
	private readonly LocalizationMapper _localizationMapper;

	#endregion

	#region Ctor

	public Menu(IWorkContext workContext,
		IMenuService menuService,
		ILocalizationService localizationService,
		MastersMapper mastersMapper,
		LocalizationMapper localizationMapper)
	{
		_workContext = workContext;
		_menuService = menuService;
		_localizationService = localizationService;
		_mastersMapper = mastersMapper;
		_localizationMapper = localizationMapper;
	}

	#endregion

	#region Methods

	public async Task<IViewComponentResult> InvokeAsync()
	{
		var user = await _workContext.GetCurrentUserAsync();
		var menusEntities = await _menuService.GetAllAsync(user);
		var resources = await _localizationService.GetAllMenuResourceAsync(user.LanguageId);
		var model = new MenuModel()
		{
			MenuItems = menusEntities.Select(x => _mastersMapper.ToModel(x)).ToList(),
			LocaleResources = resources.Select(x => _localizationMapper.ToModel(x)).ToList()
		};

		return View(model);
	}

	#endregion
}
