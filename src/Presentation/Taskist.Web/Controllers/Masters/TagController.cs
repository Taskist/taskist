using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Taskist.Core.Enums;
using Taskist.Service.Localization;
using Taskist.Service.Masters;
using Taskist.Service.Security;
using Taskist.Web.Controllers.Common;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Helpers.Mapping;
using Taskist.Web.Models.Common;
using Taskist.Web.Models.Datatable;
using Taskist.Web.Models.Masters;

namespace Taskist.Web.Controllers.Masters;

[CheckPermission(nameof(PermissionProvider.ManageTags))]
public class TagController : BaseController
{
    #region Fields

    protected readonly ITagService _tagService;
    protected readonly ILocalizationService _localizationService;
    protected readonly MastersMapper _mapper;

    #endregion

    #region Ctor

    public TagController(ITagService tagService,
        ILocalizationService localizationService,
        MastersMapper mapper)
    {
        _tagService = tagService;
        _localizationService = localizationService;
        _mapper = mapper;
    }

    #endregion

    #region Actions

    public async Task<IActionResult> Index()
    {
        return View();
    }

    public async Task<IActionResult> Create()
    {
        return PartialView(new TagModel());
    }

    [HttpPost]
    public async Task<IActionResult> Create(TagModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = _mapper.ToEntity(model);

            await _tagService.InsertAsync(entity);

            return Json(new JsonResponseModel
            {
                Status = HttpStatusCode.Success,
                Message = await _localizationService.GetResourceAsync("Message.SaveSuccess")
            });
        }

        return Json(new JsonResponseModel
        {
            Status = ModelState.IsValid ? HttpStatusCode.InternalServerError : HttpStatusCode.ValidationError,
            Message = await _localizationService.GetResourceAsync("Error.Failed"),
            Errors = ModelState.AllErrors()
        });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _tagService.GetByIdAsync(id);
        if (entity == null)
            return NoDataPartial();

        var model = _mapper.ToModel(entity);

        return PartialView(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(TagModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = await _tagService.GetByIdAsync(model.Id);
            _mapper.Update(model, entity);

            await _tagService.UpdateAsync(entity);

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

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _tagService.GetByIdAsync(id);
        if (entity == null)
            return Json(new JsonResponseModel
            {
                Status = HttpStatusCode.NoData,
                Message = await _localizationService.GetResourceAsync("FormNoData.Description")
            });

        await _tagService.DeleteAsync(entity);

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCode.Success,
            Message = await _localizationService.GetResourceAsync("Message.DeleteSuccess")
        });
    }

    public async Task<IActionResult> Merge(int id)
    {
        var entity = await _tagService.GetByIdAsync(id);
        if (entity == null)
            return NoDataPartial();

        var model = new TagMergeModel { SourceId = entity.Id, SourceName = entity.Name };

        foreach (var tag in (await _tagService.GetAllActiveAsync()).Where(x => x.Id != id).OrderBy(x => x.Name))
        {
            model.AvailableTargets.Add(new SelectListItem { Text = tag.Name, Value = tag.Id.ToString() });
        }

        return PartialView(model);
    }

    [HttpPost]
    public async Task<IActionResult> Merge(TagMergeModel model)
    {
        if (ModelState.IsValid && model.TargetId > 0 && model.TargetId != model.SourceId)
        {
            await _tagService.MergeAsync(model.SourceId, model.TargetId);

            return Json(new JsonResponseModel
            {
                Status = HttpStatusCode.Success,
                Message = await _localizationService.GetResourceAsync("Message.UpdateSuccess")
            });
        }

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCode.ValidationError,
            Message = await _localizationService.GetResourceAsync("Error.Failed"),
            Errors = ModelState.AllErrors()
        });
    }

    #endregion

    #region Data

    [HttpPost]
    public async Task<IActionResult> DataRead(DataTableRequest request)
    {
        var data = await _tagService.GetPagedListAsync(request.SearchValue, request.Start,
            request.Length, request.SortColumn, request.SortDirection);

        return Json(new
        {
            request.Draw,
            data = data.Select(x => _mapper.ToModel(x)),
            recordsFiltered = data.TotalCount,
            recordsTotal = data.TotalCount
        });
    }

    #endregion
}
