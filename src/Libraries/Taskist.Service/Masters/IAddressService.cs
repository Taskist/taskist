using Taskist.Core.Domain.Masters;

namespace Taskist.Service.Masters;

public interface IAddressService
{
    Task<Address> GetByIdAsync(int id);

    Task<Address> InsertAsync(Address entity);

    Task UpdateAsync(Address entity);

    Task DeleteAsync(Address entity);
}
