using Microsoft.AspNetCore.Http;
using Taskist.Core.Domain.Masters;

namespace Taskist.Service.Masters;

public interface IDocumentService
{
    Task<Document> GetByIdAsync(int id);

    /// <summary>
    /// Checks the upload against the permitted extensions and size limit.
    /// </summary>
    bool IsAllowed(IFormFile file, out string rejectReason);

    Task<Document> InsertAsync(IFormFile file);

    Task UpdateAsync(int documentId, IFormFile file);

    Task DeleteAsync(Document entity);
}
