using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;

namespace Taskist.Service.Masters;

public interface IEmailTemplateService
{
    Task<IPagedList<EmailTemplate>> GetPagedListAsync(string search = "", int pageIndex = 0, int pageSize = int.MaxValue,
        string sortColumn = "", string sortDirection = "");

    Task<EmailTemplate> GetByIdAsync(int id);

    Task<EmailTemplate> GetByNameAsync(string name);

    Task<IList<EmailTemplate>> GetByEmailAccountIdAsync(int emailAccountId);

    Task InsertAsync(EmailTemplate entity);

    Task UpdateAsync(EmailTemplate entity);
}