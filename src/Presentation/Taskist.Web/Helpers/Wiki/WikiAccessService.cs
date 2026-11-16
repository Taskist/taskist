using Taskist.Core.Common;
using Taskist.Core.Domain.Wiki;
using Taskist.Service.Masters;
using Taskist.Service.Security;
using Taskist.Service.Users;
using Taskist.Service.Wiki;

namespace Taskist.Web.Helpers.Wiki;

public class WikiAccessService : IWikiAccessService
{
    #region Fields

    private readonly IWorkContext _workContext;
    private readonly IPermissionService _permissionService;
    private readonly IUserService _userService;
    private readonly IWikiService _wikiService;
    private readonly IWikiCategoryService _wikiCategoryService;
    private readonly IDocumentService _documentService;

    #endregion

    #region Ctor

    public WikiAccessService(IWorkContext workContext,
        IPermissionService permissionService,
        IUserService userService,
        IWikiService wikiService,
        IWikiCategoryService wikiCategoryService,
        IDocumentService documentService)
    {
        _workContext = workContext;
        _permissionService = permissionService;
        _userService = userService;
        _wikiService = wikiService;
        _wikiCategoryService = wikiCategoryService;
        _documentService = documentService;
    }

    #endregion

    #region Methods

    public async Task<bool> CanViewProjectAsync(int projectId)
    {
        if (projectId <= 0)
            return false;

        var user = await _workContext.GetCurrentUserAsync();
        if (user == null || !await _permissionService.AuthorizeAsync(PermissionProvider.ManageWiki))
            return false;

        return (await _userService.GetAllAccessibleProjects(user.Id)).Any(p => p.Id == projectId);
    }

    public async Task<bool> CanEditProjectAsync(int projectId)
    {
        if (projectId <= 0)
            return false;

        var user = await _workContext.GetCurrentUserAsync();
        if (user == null)
            return false;

        var map = await _userService.GetProjectMapping(user.Id, projectId);
        return map != null && map.CanAddWiki;
    }

    public async Task<bool> CanViewPageAsync(int wikiId)
    {
        var projectId = await ProjectOfPageAsync(wikiId);
        return projectId.HasValue && await CanViewProjectAsync(projectId.Value);
    }

    public async Task<bool> CanEditPageAsync(int wikiId)
    {
        var projectId = await ProjectOfPageAsync(wikiId);
        return projectId.HasValue && await CanEditProjectAsync(projectId.Value);
    }

    public async Task<WikiCategory?> GetViewableCategoryAsync(int categoryId)
    {
        var category = categoryId > 0 ? await _wikiCategoryService.GetByIdAsync(categoryId) : null;
        return category != null && await CanViewProjectAsync(category.ProjectId) ? category : null;
    }

    public async Task<bool> CanPlaceCategoryUnderAsync(WikiCategory category, int? newParentId)
    {
        if (!newParentId.HasValue)
            return true;

        var parent = await _wikiCategoryService.GetByIdAsync(newParentId.Value);
        if (parent == null || !WikiTreeRules.IsSameProject(category.ProjectId, parent.ProjectId))
            return false;

        var parentOf = (await _wikiCategoryService.GetAllActiveAsync())
            .Where(c => c.ProjectId == category.ProjectId)
            .ToDictionary(c => c.Id, c => c.ParentCategoryId);

        return category.Id <= 0 || !WikiTreeRules.WouldCreateCycle(category.Id, parent.Id, parentOf);
    }

    public async Task<bool> IsCategoryInProjectAsync(int categoryId, int projectId)
    {
        var category = categoryId > 0 ? await _wikiCategoryService.GetByIdAsync(categoryId) : null;
        return category != null && WikiTreeRules.IsSameProject(category.ProjectId, projectId);
    }

    public async Task<bool> CanViewImageAsync(int documentId)
    {
        if (documentId <= 0 || await _documentService.IsTaskAttachmentAsync(documentId) || await _documentService.IsProfilePhotoAsync(documentId))
            return false;

        var image = await _wikiService.GetImageAsync(documentId);
        if (image != null)
        {
            var user = await _workContext.GetCurrentUserAsync();
            return (user != null && user.Id == image.UploadedById) || await CanViewProjectAsync(image.ProjectId);
        }

        var ownerPageId = await _wikiService.GetImageOwnerPageIdAsync(documentId);
        return ownerPageId.HasValue && await CanViewPageAsync(ownerPageId.Value);
    }

    public async Task<int?> GetEditablePageProjectAsync(int wikiId)
    {
        var projectId = await ProjectOfPageAsync(wikiId);
        return projectId.HasValue && await CanEditProjectAsync(projectId.Value) ? projectId : null;
    }

    #endregion

    #region Utilities

    private async Task<int?> ProjectOfPageAsync(int wikiId)
    {
        var wiki = wikiId > 0 ? await _wikiService.GetByIdAsync(wikiId) : null;
        if (wiki == null)
            return null;

        var category = await _wikiCategoryService.GetByIdAsync(wiki.CategoryId);
        return category?.ProjectId;
    }

    #endregion
}
