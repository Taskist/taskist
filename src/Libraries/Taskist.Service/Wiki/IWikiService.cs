using Taskist.Core.Domain.Wiki;

namespace Taskist.Service.Wiki
{
    public interface IWikiService
    {
        Task<IList<WikiDocument>> GetAllByCategoryAsync(int categoryId);

        Task<WikiDocument?> GetByIdAsync(int id);

        Task InsertAsync(WikiDocument document);

        Task UpdateAsync(WikiDocument updated, string? versionNote = null);

        Task<bool> DeleteAsync(int id);

        Task<IList<WikiDocumentVersion>> GetVersionsAsync(int wikiId);

        Task<bool> RestoreVersionAsync(int wikiId, int versionId, int modifiedBy);

        Task<int?> GetImageOwnerPageIdAsync(int documentId);
    }
}
