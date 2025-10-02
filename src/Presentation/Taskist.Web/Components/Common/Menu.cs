using AutoMapper;
using Taskist.Core.Common;
using Taskist.Service.Localization;
using Taskist.Service.Masters;
using Microsoft.AspNetCore.Mvc;
using Taskist.Web.Models.Localization;
using Taskist.Web.Models.Masters;

namespace Taskist.Web.Components.Common;

public class Menu : ViewComponent
{
	#region Fields

	private readonly IWorkContext _workContext;
	private readonly IMenuService _menuService;
	private readonly ILocalizationService _localizationService;
	private readonly IMapper _mapper;

	#endregion

	#region Ctor

	public Menu(IWorkContext workContext,
		IMenuService menuService,
		ILocalizationService localizationService,
		IMapper mapper)
	{
		_workContext = workContext;
		_menuService = menuService;
		_localizationService = localizationService;
		_mapper = mapper;
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
			MenuItems = menusEntities.Select(x => _mapper.Map<MenuItemModel>(x)).ToList(),
			LocaleResources = resources.Select(x => _mapper.Map<LocaleResourceModel>(x)).ToList()
		};

		return View(model);
	}

	#endregion
}