using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;
using System.Linq.Dynamic.Core;
using Taskist.Core.Caching;
using Taskist.Core.Common;
using Taskist.Core.Domain.Localization;
using Taskist.Data.Repository;
using Taskist.Service.Common;
using Taskist.Service.Logging;

namespace Taskist.Service.Localization;

public class LocalizationService : ILocalizationService
{
    #region Fields

    protected readonly IRepository<LocaleResource> _localeResourceRepository;
    protected readonly IWorkContext _workContext;
    protected readonly ICacheManager _cacheManager;
    protected readonly ILogService _logService;

    #endregion

    #region Ctor
    public LocalizationService(IRepository<LocaleResource> localeResourceRepository,
        IWorkContext workContext,
        ICacheManager cacheManager,
        ILogService logService)
    {
        _localeResourceRepository = localeResourceRepository;
        _workContext = workContext;
        _cacheManager = cacheManager;
        _logService = logService;
    }
    #endregion

    #region Methods

    public async Task<IPagedList<LocaleResource>> GetPagedListAsync(int languageId, string search = "", int pageIndex = 0, int pageSize = int.MaxValue,
        string sortColumn = "", string sortDirection = "")
    {
        return await _localeResourceRepository.GetAllPagedAsync(query =>
        {
            query = query.Where(x => x.LanguageId == languageId);
            query = query.OrderBy($"{sortColumn} {sortDirection}");

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(
                        c => c.ResourceKey.Contains(search) ||
                        c.ResourceValue.Contains(search));

            return query;
        }, pageIndex, pageSize);
    }
    public async Task<IList<LocaleResource>> GetAllAsync()
    {
        return await _localeResourceRepository.GetAllAsync();
    }

    public async Task<IList<LocaleResource>> GetByLanguageIdAsync(int languageId)
    {
        var query = from c in _localeResourceRepository.Table
                    orderby c.Id
                    where c.LanguageId == languageId
                    select c;

        return await query.ToListAsync();
    }

    public async Task<LocaleResource> GetByIdAsync(int id)
    {
        return id == 0 ? null : await _localeResourceRepository.GetByIdAsync(id);
    }

    public async Task<string> GetResourceAsync(int languageId, string resourceKey, bool logIfNotFound = false,
        string defaultValue = "", bool returnEmptyIfNotFound = false)
    {
        var result = string.Empty;
        resourceKey ??= string.Empty;
        resourceKey = resourceKey.Trim().ToLowerInvariant();

        var key = string.Format(ServiceConstant.LocaleStringResourcesByNameCacheKey, languageId, resourceKey);

        var query = from l in _localeResourceRepository.Table
                    where l.ResourceKey == resourceKey
                          && l.LanguageId == languageId
                    select l.ResourceValue;

        var lsr = await _cacheManager.GetAsync(key, async () => await query.FirstOrDefaultAsync());

        if (lsr != null)
            result = lsr;

        if (!string.IsNullOrEmpty(result))
            return result;

        if (logIfNotFound)
            await _logService.WarningAsync($"Resource string ({resourceKey}) is not found. Language ID = {languageId}");

        if (!string.IsNullOrEmpty(defaultValue))
        {
            result = defaultValue;
        }
        else
        {
            if (!returnEmptyIfNotFound)
                result = resourceKey;
        }

        return result;
    }

    public async Task<string> GetResourceAsync(string resourceKey)
    {
        if (string.IsNullOrWhiteSpace(resourceKey))
            return null;

        var language = await _workContext.GetCurrentUserLanguageAsync();

        if (language != null)
            return await GetResourceAsync(language.Id, resourceKey);

        return string.Empty;
    }

    public async Task<LocaleResource> GetResourceByKeyAsync(int languageId, string resourceKey)
    {
        if (string.IsNullOrWhiteSpace(resourceKey))
            return null;

        var query = from c in _localeResourceRepository.Table
                    where c.LanguageId == languageId && c.ResourceKey == resourceKey
                    select c;

        return await query.FirstOrDefaultAsync();
    }

    public async Task<IList<LocaleResource>> GetAllMenuResourceAsync(int languageId)
    {
        var key = ServiceConstant.MenuResourceCacheKey;

        var query = from c in _localeResourceRepository.Table
                    where c.LanguageId == languageId && c.ResourceKey.ToLower().EndsWith(".menu")
                    select c;

        return await _cacheManager.GetAsync(key, async () => await query.ToListAsync());
    }

    public async Task InsertAsync(LocaleResource entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _localeResourceRepository.InsertAsync(entity);
    }

    public async Task InsertAsync(IList<LocaleResource> entities)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        await _localeResourceRepository.InsertAsync(entities);
    }

    public async Task UpdateAsync(LocaleResource entity)
    {

        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _localeResourceRepository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(LocaleResource entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _localeResourceRepository.DeleteAsync(entity);
    }

    #endregion
}
