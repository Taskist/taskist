using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;

namespace Taskist.Service.Masters;

public interface ICustomFieldService
{
    Task<IPagedList<CustomField>> GetPagedListAsync(int projectId, string search = "", int pageIndex = 0,
        int pageSize = int.MaxValue);

    Task<IList<CustomField>> GetAllAsync(int projectId, int placement);

    Task<IList<CustomField>> GetAllMandatoryAsync(int projectId);

    Task<CustomField> GetByIdAsync(int id);

    Task<CustomField> GetByLabelAsync(int projectId, string label);

    Task InsertAsync(CustomField entity);

    Task InsertAsync(List<CustomField> entities);

    Task UpdateAsync(CustomField entity);

    Task DeleteAsync(CustomField entity);
}
