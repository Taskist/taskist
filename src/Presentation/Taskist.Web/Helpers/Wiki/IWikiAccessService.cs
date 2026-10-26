using Taskist.Core.Domain.Wiki;

namespace Taskist.Web.Helpers.Wiki;

public interface IWikiAccessService
{
    Task<bool> CanViewProjectAsync(int projectId);

    Task<bool> CanEditProjectAsync(int projectId);

    Task<bool> CanViewPageAsync(int wikiId);

    Task<bool> CanEditPageAsync(int wikiId);

    Task<WikiCategory?> GetViewableCategoryAsync(int categoryId);

    Task<bool> CanPlaceCategoryUnderAsync(WikiCategory category, int? newParentId);

    Task<bool> IsCategoryInProjectAsync(int categoryId, int projectId);

    Task<bool> CanViewImageAsync(int documentId);
}
