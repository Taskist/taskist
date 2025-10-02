using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;
using System.Linq.Dynamic.Core;
using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Data.Repository;

namespace Taskist.Service.Masters;

public class ClientService : IClientService
{
    #region Fields

    protected readonly IRepository<Client> _clientRepository;

    #endregion

    #region Ctor
    public ClientService(IRepository<Client> clientRepository)
    {
        _clientRepository = clientRepository;
    }
    #endregion

    #region Methods

    public async Task<IPagedList<Client>> GetPagedListAsync(string search = "", int pageIndex = 0,
    int pageSize = int.MaxValue, string sortColumn = "", string sortDirection = "")
    {
        return await _clientRepository.GetAllPagedAsync(query =>
        {
            query = query.Where(x => !x.Deleted);
            query = query.OrderBy($"{sortColumn} {sortDirection}");

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.Name.Contains(search));

            return query;
        }, pageIndex, pageSize);
    }
    public async Task<IList<Client>> GetAllAsync(bool showDeleted = false)
    {
        return await _clientRepository.GetAllAsync(includeDeleted: showDeleted);
    }

    public async Task<IList<Client>> GetAllActiveAsync(bool showDeleted = false)
    {
        return await _clientRepository.GetAllAsync(q => q.Where(x => x.Active), showDeleted);
    }

    public async Task<Client> GetByIdAsync(int id)
    {
        return id == 0 ? null : await _clientRepository.GetByIdAsync(id);
    }

    public async Task<Client> GetByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var query = from c in _clientRepository.Table
                    orderby c.Id
                    where !c.Deleted && c.Name == name
                    select c;
        return await query.FirstOrDefaultAsync();
    }

    public async Task InsertAsync(Client entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _clientRepository.InsertAsync(entity);
    }

    public async Task InsertAsync(IList<Client> entities)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        await _clientRepository.InsertAsync(entities);
    }

    public async Task UpdateAsync(Client entity)
    {

        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _clientRepository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(Client entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _clientRepository.DeleteAsync(entity);
    }

    #endregion
}
