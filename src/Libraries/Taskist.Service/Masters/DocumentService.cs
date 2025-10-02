using Taskist.Core.Domain.Masters;
using Taskist.Data.Repository;
using Microsoft.AspNetCore.Http;

namespace Taskist.Service.Masters;

public class DocumentService : IDocumentService
{
    #region Fields

    protected readonly IRepository<Document> _documentRepository;

    #endregion

    #region Ctor
    public DocumentService(IRepository<Document> documentRepository)
    {
        _documentRepository = documentRepository;
    }
    #endregion

    #region Methods

    public async Task<Document> GetByIdAsync(int id)
    {
        return id == 0 ? null : await _documentRepository.GetByIdAsync(id);
    }

    public async Task<Document> InsertAsync(IFormFile file)
    {
        if (file == null)
            throw new ArgumentNullException();

        var fileData = GetBytesFromFile(file);

        if (fileData != null && fileData.Length > 0)
        {
            var entity = new Document
            {
                FileName = Path.GetFileNameWithoutExtension(file.FileName),
                FileData = fileData,
                ContentType = file.ContentType,
                FileSize = file.Length,
                Extension = Path.GetExtension(file.FileName)
            };

            return await _documentRepository.InsertAndGetAsync(entity);
        }

        return new Document();
    }

    public async Task UpdateAsync(int documentId, IFormFile file)
    {

        var entity = await _documentRepository.GetByIdAsync(documentId);

        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var fileData = GetBytesFromFile(file);

        if (fileData != null && fileData.Length > 0)
        {
            entity.FileName = Path.GetFileNameWithoutExtension(file.FileName);
            entity.FileData = fileData;
            entity.ContentType = file.ContentType;
            entity.FileSize = file.Length;
            entity.Extension = Path.GetExtension(file.FileName);

            await _documentRepository.UpdateAsync(entity);
        }
    }

    public async Task DeleteAsync(Document entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _documentRepository.DeleteAsync(entity);
    }

    #endregion

    #region Helpers

    private byte[] GetBytesFromFile(IFormFile file)
    {
        try
        {
            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                return ms.ToArray();
            }
        }
        catch (Exception)
        {
            return null;
        }
    }

    #endregion
}
