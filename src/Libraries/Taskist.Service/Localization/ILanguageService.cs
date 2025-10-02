using Taskist.Core.Common;
using Taskist.Core.Domain.Localization;

namespace Taskist.Service.Localization;

public interface ILanguageService
{
    Task<IPagedList<Language>> GetPagedListAsync(string search = "", int pageIndex = 0, int pageSize = int.MaxValue,
       string sortColumn = "", string sortDirection = "");

    Task<IList<Language>> GetAllAsync();

    Task<IList<Language>> GetAllActiveAsync();

    Task<Language> GetByIdAsync(int id);

    Task<Language> GetByNameAsync(string name);

    Task<Language> GetByCultureAsync(string culture);

    Task InsertAsync(Language entity);

    Task InsertAsync(IList<Language> entities);

    Task UpdateAsync(Language entity);

    Task DeleteAsync(Language entity);
}
