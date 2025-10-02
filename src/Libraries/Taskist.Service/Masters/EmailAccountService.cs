using System.Linq.Dynamic.Core;
using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Data.Repository;
using LinqToDB;

namespace Taskist.Service.Masters;

public class EmailAccountService : IEmailAccountService
{
    #region Fields

    protected readonly IRepository<EmailAccount> _emailAccountRepository;

    #endregion

    #region Ctor
    public EmailAccountService(IRepository<EmailAccount> emailAccountRepository)
    {
        _emailAccountRepository = emailAccountRepository;
    }
    #endregion

    #region Methods

    public async Task<IPagedList<EmailAccount>> GetPagedListAsync(string search = "", int pageIndex = 0, int pageSize = int.MaxValue,
        string sortColumn = "", string sortDirection = "")
    {
        return await _emailAccountRepository.GetAllPagedAsync(query =>
        {
            query = query.OrderBy($"{sortColumn} {sortDirection}");

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.Name.Contains(search));

            return query;
        }, pageIndex, pageSize);
    }
    public async Task<IList<EmailAccount>> GetAllAsync()
    {
        return await _emailAccountRepository.GetAllAsync();
    }

    public async Task<IList<EmailAccount>> GetAllActiveAsync()
    {
        return await _emailAccountRepository.GetAllAsync(q => q.Where(x => x.Active));
    }

    public async Task<EmailAccount> GetFirstActiveAsync()
    {
        var query = from c in _emailAccountRepository.Table
                    orderby c.Id
                    select c;
        return await query.FirstOrDefaultAsync();
    }

    public async Task<EmailAccount> GetByIdAsync(int id)
    {
        return id == 0 ? null : await _emailAccountRepository.GetByIdAsync(id);
    }

    public async Task<EmailAccount> GetByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var query = from c in _emailAccountRepository.Table
                    orderby c.Id
                    where c.Name == name
                    select c;
        return await query.FirstOrDefaultAsync();
    }

    public async Task InsertAsync(EmailAccount entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _emailAccountRepository.InsertAsync(entity);
    }

    public async Task UpdateAsync(EmailAccount entity)
    {

        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _emailAccountRepository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(EmailAccount entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _emailAccountRepository.DeleteAsync(entity);
    }

    #endregion
}
