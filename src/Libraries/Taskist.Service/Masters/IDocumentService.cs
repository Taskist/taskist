using Taskist.Core.Domain.Masters;
using Microsoft.AspNetCore.Http;

namespace Taskist.Service.Masters;

public interface IDocumentService
{
    Task<Document> GetByIdAsync(int id);

    Task<bool> IsTaskAttachmentAsync(int documentId);

    Task<bool> IsProfilePhotoAsync(int documentId);

    Task<Document> InsertAsync(IFormFile file);

    Task<Document> InsertAsync(byte[] data, string fileName, string contentType);

    Task UpdateAsync(int documentId, IFormFile file);

    Task DeleteAsync(Document entity);
}
