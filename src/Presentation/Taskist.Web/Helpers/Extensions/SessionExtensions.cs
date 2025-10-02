using Newtonsoft.Json;

namespace Taskist.Web.Helpers.Extensions;

public static class SessionExtensions
{
    public static async Task SetAsync<T>(this ISession session, string key, T value)
    {
        await LoadAsync(session);
        session.SetString(key, JsonConvert.SerializeObject(value));
    }

    public static async Task<T> GetAsync<T>(this ISession session, string key)
    {
        await LoadAsync(session);
        var value = session.GetString(key);
        return value == null ? default : JsonConvert.DeserializeObject<T>(value);
    }

    public static async Task RemoveAsync(this ISession session, string key)
    {
        await LoadAsync(session);
        session.Remove(key);
    }

    public static async Task ClearAsync(this ISession session)
    {
        await LoadAsync(session);
        session.Clear();
    }

    public static async Task LoadAsync(ISession session)
    {
        try
        {
            await session.LoadAsync();
        }
        catch
        {
        }
    }
}
