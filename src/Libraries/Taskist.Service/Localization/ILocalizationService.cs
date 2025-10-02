using Taskist.Core.Common;
using Taskist.Core.Domain.Localization;

namespace Taskist.Service.Localization;

public interface ILocalizationService
{
    Task<IPagedList<LocaleResource>> GetPagedListAsync(int languageId, string search = "", int pageIndex = 0, int pageSize = int.MaxValue,
       string sortColumn = "", string sortDirection = "");

    Task<IList<LocaleResource>> GetAllAsync();

    Task<IList<LocaleResource>> GetByLanguageIdAsync(int id);

    Task<LocaleResource> GetByIdAsync(int id);

    Task<string> GetResourceAsync(int languageId, string resourceKey, bool logIfNotFound = false,
        string defaultValue = "", bool returnEmptyIfNotFound = false);

    Task<string> GetResourceAsync(string resourceKey);

    Task<LocaleResource> GetResourceByKeyAsync(int languageId, string resourceKey);

    Task<IList<LocaleResource>> GetAllMenuResourceAsync(int languageId);

    Task InsertAsync(LocaleResource entity);

    Task InsertAsync(IList<LocaleResource> entities);

    Task UpdateAsync(LocaleResource entity);

    Task DeleteAsync(LocaleResource entity);
}
