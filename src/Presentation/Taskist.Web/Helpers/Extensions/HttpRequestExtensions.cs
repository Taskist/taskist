using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Taskist.Web.Helpers.Extensions;

public static class HttpRequestExtensions
{
    public static bool IsPostRequest(this HttpRequest request)
    {
        return request.Method.Equals(WebRequestMethods.Http.Post, StringComparison.InvariantCultureIgnoreCase);
    }

    public static bool IsGetRequest(this HttpRequest request)
    {
        return request.Method.Equals(WebRequestMethods.Http.Get, StringComparison.InvariantCultureIgnoreCase);
    }

    public static async Task<StringValues> GetFormValueAsync(this HttpRequest request, string formKey)
    {
        if (!request.HasFormContentType)
            return new StringValues();

        var form = await request.ReadFormAsync();

        return form[formKey];
    }

    public static async Task<bool> IsFormKeyExistsAsync(this HttpRequest request, string formKey)
    {
        return await IsFormAnyAsync(request, key => key.Equals(formKey));
    }

    public static async Task<bool> IsFormAnyAsync(this HttpRequest request, Func<string, bool> predicate = null)
    {
        if (!request.HasFormContentType)
            return false;

        var form = await request.ReadFormAsync();

        return predicate == null ? form.Count != 0 : form.Keys.Any(predicate);
    }

    public static async Task<(bool keyExists, StringValues formValue)> TryGetFormValueAsync(this HttpRequest request, string formKey)
    {
        if (!request.HasFormContentType)
            return (false, default);

        var form = await request.ReadFormAsync();

        var flag = form.TryGetValue(formKey, out var formValue);

        return (flag, formValue);
    }

    public static async Task<IFormFile> GetFirstOrDefaultFileAsync(this HttpRequest request)
    {
        if (!request.HasFormContentType)
            return default;

        var form = await request.ReadFormAsync();

        return form.Files.FirstOrDefault();
    }
}
