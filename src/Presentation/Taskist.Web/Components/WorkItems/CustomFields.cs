using AutoMapper;
using Taskist.Core.Common;
using Taskist.Service.Masters;
using Taskist.Service.WorkItems;
using Microsoft.AspNetCore.Mvc;
using Taskist.Web.Models.Masters;

namespace Taskist.Web.Components.WorkItems;

public class CustomFields : ViewComponent
{
    #region Fields

    protected readonly ICustomFieldService _customFieldService;
    protected readonly IBacklogItemService _backlogService;
    protected readonly IGenericAttributeService _genericAttributeService;
    protected readonly IWorkContext _workContext;
    protected readonly IMapper _mapper;

    #endregion

    #region Ctor

    public CustomFields(ICustomFieldService customFieldService,
        IBacklogItemService backlogService,
        IGenericAttributeService genericAttributeService,
        IWorkContext workContext,
        IMapper mapper)
    {
        _customFieldService = customFieldService;
        _backlogService = backlogService;
        _genericAttributeService = genericAttributeService;
        _workContext = workContext;
        _mapper = mapper;
    }

    #endregion

    #region Methods

    public async Task<IViewComponentResult> InvokeAsync(int backLogId, int placement)
    {
        var loggedUser = await _workContext.GetCurrentUserAsync();
        var lastAccessedProjectId = await _genericAttributeService.GetAttributeAsync<int>(loggedUser, Constant.ActiveProjectSession);

        var customFields = await _customFieldService.GetAllAsync(lastAccessedProjectId, placement);
        var customFieldValues = await _backlogService.GetAllCustomFieldValuesAsync(backLogId);

        var model = new CustomFieldsViewModel()
        {
            BacklogId = backLogId,
            CustomFields = customFields.Select(x => _mapper.Map<CustomFieldRenderModel>(x)).ToList(),
            Values = customFieldValues.Select(x => _mapper.Map<CustomFieldValueModel>(x)).ToList()
        };

        return View(model);
    }

    #endregion
}