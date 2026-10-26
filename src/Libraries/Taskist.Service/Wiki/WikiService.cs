using Taskist.Core.Domain.Wiki;
using Taskist.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace Taskist.Service.Wiki
{
    public class WikiService : IWikiService
    {
        #region Fields

        protected readonly IRepository<WikiDocument> _wikiDocumentRepository;
        protected readonly IRepository<WikiDocumentVersion> _wikiDocumentVersionRepository;

        #endregion

        #region Ctor

        public WikiService(IRepository<WikiDocument> wikiDocumentRepository,
            IRepository<WikiDocumentVersion> wikiDocumentVersionRepository)
        {
            _wikiDocumentRepository = wikiDocumentRepository;
            _wikiDocumentVersionRepository = wikiDocumentVersionRepository;
        }

        #endregion

        #region Methods

        public async Task<IList<WikiDocument>> GetAllByCategoryAsync(int categoryId)
        {
            return await _wikiDocumentRepository.GetAllAsync(q =>
                q.Where(x => x.CategoryId == categoryId)
                .OrderByDescending(o => o.CreatedOn), false);
        }

        public async Task<WikiDocument?> GetByIdAsync(int id)
        {
            return id == 0 ? null : await _wikiDocumentRepository.GetByIdAsync(id);
        }

        public async Task InsertAsync(WikiDocument document)
        {
            document.CreatedOn = DateTime.UtcNow;
            document.ModifiedOn = DateTime.UtcNow;

            await _wikiDocumentRepository.InsertAsync(document);

            var version = new WikiDocumentVersion
            {
                WikiDocumentId = document.Id,
                Title = document.Title,
                ContentHtml = document.ContentHtml,
                CreatedOn = DateTime.UtcNow,
                ModifiedById = document.ModifiedById
            };

            await _wikiDocumentVersionRepository.InsertAsync(version);
        }

        public async Task UpdateAsync(WikiDocument doc, string? versionNote = null)
        {
            var version = new WikiDocumentVersion
            {
                WikiDocumentId = doc.Id,
                Title = doc.Title,
                ContentHtml = doc.ContentHtml,
                CreatedOn = DateTime.UtcNow,
                ModifiedById = doc.ModifiedById,
                Note = versionNote
            };

            await _wikiDocumentVersionRepository.InsertAsync(version);


            doc.Title = doc.Title;
            doc.ContentHtml = doc.ContentHtml;
            doc.CategoryId = doc.CategoryId;
            doc.ModifiedOn = DateTime.UtcNow;

            await _wikiDocumentRepository.UpdateAsync(doc);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var doc = await _wikiDocumentRepository.GetByIdAsync(id);
            if (doc == null) return false;

            await _wikiDocumentRepository.DeleteAsync(doc);

            return true;
        }

        public async Task<IList<WikiDocumentVersion>> GetVersionsAsync(int wikiId)
        {
            return await _wikiDocumentVersionRepository.Table
                .Where(v => v.WikiDocumentId == wikiId)
                .OrderByDescending(v => v.CreatedOn)
                .ToListAsync();
        }

        public async Task<bool> RestoreVersionAsync(int wikiId, int versionId, int modifiedBy)
        {
            var doc = await _wikiDocumentRepository.GetByIdAsync(wikiId);
            var version = await _wikiDocumentVersionRepository.GetByIdAsync(versionId);
            if (doc == null || version == null || version.WikiDocumentId != doc.Id)
                return false;

            var currentSnapshot = new WikiDocumentVersion
            {
                WikiDocumentId = doc.Id,
                Title = doc.Title,
                ContentHtml = doc.ContentHtml,
                CreatedOn = DateTime.UtcNow,
                ModifiedById = modifiedBy,
                Note = "Snapshot before restore"
            };
            await _wikiDocumentVersionRepository.InsertAsync(currentSnapshot);

            doc.Title = version.Title;
            doc.ContentHtml = version.ContentHtml;
            doc.ModifiedOn = DateTime.UtcNow;

            await _wikiDocumentRepository.UpdateAsync(doc);
            return true;
        }

        public async Task<int?> GetImageOwnerPageIdAsync(int documentId)
        {
            if (documentId <= 0)
                return null;

            var fragments = WikiImageReference.SearchFragments(documentId);
            var candidates = await _wikiDocumentVersionRepository.Table
                .Where(v => v.ContentHtml != null && (v.ContentHtml.Contains(fragments[0]) || v.ContentHtml.Contains(fragments[1])))
                .OrderBy(v => v.CreatedOn).ThenBy(v => v.Id)
                .Select(v => new { v.WikiDocumentId, v.ContentHtml })
                .Take(50)
                .ToListAsync();

            return candidates.FirstOrDefault(v => WikiImageReference.References(v.ContentHtml, documentId))?.WikiDocumentId;
        }

        #endregion
    }
}
