using Taskist.Core.Domain.Masters;
using Taskist.Data.Repository;

namespace Taskist.Service.Masters;

public class AddressService : IAddressService
{
    #region Fields

    protected readonly IRepository<Address> _adddressRepository;

    #endregion

    #region Ctor
    public AddressService(IRepository<Address> adddressRepository)
    {
        _adddressRepository = adddressRepository;
    }
    #endregion

    #region Methods

    public async Task<Address> GetByIdAsync(int id)
    {
        return id == 0 ? null : await _adddressRepository.GetByIdAsync(id);
    }

    public async Task<Address> InsertAsync(Address entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        return await _adddressRepository.InsertAndGetAsync(entity);
    }

    public async Task UpdateAsync(Address entity)
    {

        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _adddressRepository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(Address entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _adddressRepository.DeleteAsync(entity);
    }

    #endregion
}
