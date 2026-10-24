using Microsoft.AspNetCore.Mvc;
using Taskist.Core.Common;
using Taskist.Core.Domain.Wiki;
using Taskist.Service.Localization;
using Taskist.Service.Users;
using Taskist.Service.Wiki;
using Taskist.Web.Controllers.Common;

namespace Taskist.Web.Controllers.Resources;

public class WikiCategoryController : BaseController
{
    #region Fields

    protected readonly IWikiCategoryService _wikiCategoryService;
    protected readonly IWikiService _wikiService;
    protected readonly IUserService _userService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public WikiCategoryController(IWikiCategoryService wikiCategoryService,
        IWikiService wikiService,
        IUserService userService,
        ILocalizationService localizationService,
        IWorkContext workContext)
    {
        _wikiCategoryService = wikiCategoryService;
        _wikiService = wikiService;
        _userService = userService;
        _localizationService = localizationService;
        _workContext = workContext;
    }

    #endregion

    #region Helpers

    private async Task<bool> CanEditAsync(int projectId)
    {
        if (projectId <= 0)
            return false;

        var map = await _userService.GetProjectMapping(UserId, projectId);
        return map != null && map.CanAddWiki;
    }

    private static IActionResult Denied() => new JsonResult(new { success = false, message = "You don't have permission to edit this wiki." });

    #endregion

    #region Ajax

    [HttpPost]
    public async Task<IActionResult> AddCategory(int? parentId, string name, int projectId)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Json(new { success = false, message = "Category name is required." });

        if (projectId <= 0)
            return Json(new { success = false, message = "Please select a project." });

        if (!await CanEditAsync(projectId))
            return Denied();

        var siblings = parentId.HasValue
            ? await _wikiCategoryService.GetAllByParentCategoryIdAsync(parentId.Value, projectId)
            : await _wikiCategoryService.GetAllParentAsync(projectId);

        if (siblings.Any(c => string.Equals(c.Name, name.Trim(), StringComparison.OrdinalIgnoreCase)))
            return Json(new { success = false, message = "A folder with this name already exists here." });

        var entity = new WikiCategory
        {
            Name = name.Trim(),
            ProjectId = projectId,
            ParentCategoryId = parentId,
            Active = true,
            Deleted = false
        };

        await _wikiCategoryService.InsertAsync(entity);

        return Json(new { success = true, id = "cat_" + entity.Id, text = entity.Name, type = "default", children = true });
    }

    [HttpPost]
    public async Task<IActionResult> RenameCategory(int id, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return Json(new { success = false, message = "Name is required." });

        var cat = await _wikiCategoryService.GetByIdAsync(id);
        if (cat == null)
            return Json(new { success = false, message = "Folder not found." });

        if (!await CanEditAsync(cat.ProjectId))
            return Denied();

        cat.Name = newName.Trim();
        await _wikiCategoryService.UpdateAsync(cat);

        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> MoveCategory(int id, int? parentId)
    {
        var cat = await _wikiCategoryService.GetByIdAsync(id);
        if (cat == null)
            return Json(new { success = false, message = "Folder not found." });

        if (!await CanEditAsync(cat.ProjectId))
            return Denied();

        if (parentId == id)
            return Json(new { success = false, message = "A folder cannot be moved into itself." });

        cat.ParentCategoryId = parentId;
        await _wikiCategoryService.UpdateAsync(cat);

        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var cat = await _wikiCategoryService.GetByIdAsync(id);
        if (cat == null)
            return Json(new { success = false, message = "Folder not found." });

        if (!await CanEditAsync(cat.ProjectId))
            return Denied();

        await _wikiCategoryService.DeleteAsync(cat);

        return Json(new { success = true });
    }

    public async Task<IActionResult> GetParent(int projectId)
    {
        var categories = await _wikiCategoryService.GetAllParentAsync(projectId);

        var result = categories.Select(c => new
        {
            id = "cat_" + c.Id,
            text = c.Name,
            children = true,
            type = "default"
        });

        return Json(result);
    }

    public async Task<IActionResult> GetChildren(string id, int projectId)
    {
        if (string.IsNullOrEmpty(id))
            return Json(Enumerable.Empty<object>());

        var categoryId = int.Parse(id.Replace("cat_", ""));

        var subCategories = (await _wikiCategoryService.GetAllByParentCategoryIdAsync(categoryId, projectId))
            .Select(c => new
            {
                id = "cat_" + c.Id,
                text = c.Name,
                children = true,
                type = "default"
            });

        var wikiPages = (await _wikiService.GetAllByCategoryAsync(categoryId))
            .Select(p => new
            {
                id = "page_" + p.Id,
                text = p.Title,
                children = false,
                type = "file"
            });

        return Json(subCategories.Concat<object>(wikiPages));
    }

    #endregion
}
