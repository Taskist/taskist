using System.Linq.Dynamic.Core;
using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Data.Repository;
using LinqToDB;

namespace Taskist.Service.Masters;

public class EmailTemplateService : IEmailTemplateService
{
    #region Fields

    protected readonly IRepository<EmailTemplate> _emailTemplateReposiory;

    #endregion

    #region Ctor
    public EmailTemplateService(IRepository<EmailTemplate> emailTemplateReposiory)
    {
        _emailTemplateReposiory = emailTemplateReposiory;
    }
    #endregion

    #region Methods

    public async Task<IPagedList<EmailTemplate>> GetPagedListAsync(string search = "", int pageIndex = 0, int pageSize = int.MaxValue,
        string sortColumn = "", string sortDirection = "")
    {
        return await _emailTemplateReposiory.GetAllPagedAsync(query =>
        {
            query = query.OrderBy($"{sortColumn} {sortDirection}");

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.Name.Contains(search));

            return query;
        }, pageIndex, pageSize);
    }

    public async Task<EmailTemplate> GetByIdAsync(int id)
    {
        return id == 0 ? null : await _emailTemplateReposiory.GetByIdAsync(id);
    }

    public async Task<EmailTemplate> GetByNameAsync(string name)
    {
        var query = from c in _emailTemplateReposiory.Table
                    orderby c.Id
                    where c.Name == name
                    select c;
        return await query.FirstOrDefaultAsync();
    }

    public async Task<IList<EmailTemplate>> GetByEmailAccountIdAsync(int emailAccountId)
    {
        var query = from c in _emailTemplateReposiory.Table
                    orderby c.Id
                    where c.EmailAccountId == emailAccountId
                    select c;
        return await query.ToListAsync();
    }

    public async Task InsertAsync(EmailTemplate entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _emailTemplateReposiory.InsertAsync(entity);
    }

    public async Task UpdateAsync(EmailTemplate entity)
    {

        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _emailTemplateReposiory.UpdateAsync(entity);
    }

    #endregion
}
