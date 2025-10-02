using System.Linq.Expressions;
using Taskist.Core.Domain.Common;
using Taskist.Core.Domain.Masters;

namespace Taskist.Service.Masters;

public partial interface ISettingService
{
    Task<Setting> GetSettingByIdAsync(int settingId);

    Setting GetSettingById(int settingId);

    Task DeleteSettingAsync(Setting setting);

    void DeleteSetting(Setting setting);

    Task DeleteSettingsAsync(IList<Setting> settings);

    Task<Setting> GetSettingAsync(string key, bool loadSharedValueIfNotFound = false);

    Setting GetSetting(string key, bool loadSharedValueIfNotFound = false);

    Task<T> GetSettingByKeyAsync<T>(string key, T defaultValue = default,
         bool loadSharedValueIfNotFound = false);

    T GetSettingByKey<T>(string key, T defaultValue = default,
         bool loadSharedValueIfNotFound = false);

    Task SetSettingAsync<T>(string key, T value, bool clearCache = true);

    void SetSetting<T>(string key, T value, bool clearCache = true);

    Task<IList<Setting>> GetAllSettingsAsync();

    IList<Setting> GetAllSettings();

    Task<bool> SettingExistsAsync<T, TPropType>(T settings,
        Expression<Func<T, TPropType>> keySelector)
        where T : ISettings, new();

    bool SettingExists<T, TPropType>(T settings,
        Expression<Func<T, TPropType>> keySelector)
        where T : ISettings, new();

    Task<T> LoadSettingAsync<T>() where T : ISettings, new();

    T LoadSetting<T>() where T : ISettings, new();

    Task<ISettings> LoadSettingAsync(Type type);

    ISettings LoadSetting(Type type);

    Task SaveSettingAsync<T>(T settings) where T : ISettings, new();

    void SaveSetting<T>(T settings) where T : ISettings, new();

    Task SaveSettingAsync<T, TPropType>(T settings,
        Expression<Func<T, TPropType>> keySelector,
        bool clearCache = true) where T : ISettings, new();

    void SaveSetting<T, TPropType>(T settings,
        Expression<Func<T, TPropType>> keySelector,
        bool clearCache = true) where T : ISettings, new();

    Task SaveSettingOverridablePerStoreAsync<T, TPropType>(T settings,
        Expression<Func<T, TPropType>> keySelector,
        bool clearCache = true) where T : ISettings, new();

    Task InsertSettingAsync(Setting setting, bool clearCache = true);

    void InsertSetting(Setting setting, bool clearCache = true);

    Task UpdateSettingAsync(Setting setting, bool clearCache = true);

    void UpdateSetting(Setting setting, bool clearCache = true);

    Task DeleteSettingAsync<T>() where T : ISettings, new();

    Task DeleteSettingAsync<T, TPropType>(T settings,
        Expression<Func<T, TPropType>> keySelector) where T : ISettings, new();

    Task ClearCacheAsync();

    void ClearCache();

    string GetSettingKey<TSettings, T>(TSettings settings, Expression<Func<TSettings, T>> keySelector)
        where TSettings : ISettings, new();
}