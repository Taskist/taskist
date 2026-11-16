using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Taskist.Core.Dtos.Git;

namespace Taskist.Service.Git;

public class GitHubAppTokenService : IGitHubAppTokenService
{
    #region Fields

    private static readonly ConcurrentDictionary<string, (string Token, DateTime ExpiresOn)> Tokens = new();
    private static readonly ConcurrentDictionary<string, long> Installations = new();

    #endregion

    #region Methods

    public string CreateAppJwt(string appId, string privateKeyPem)
    {
        var now = DateTimeOffset.UtcNow;
        var header = Base64Url(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { alg = "RS256", typ = "JWT" })));
        var payload = Base64Url(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new
        {
            iat = now.AddSeconds(-60).ToUnixTimeSeconds(),
            exp = now.AddMinutes(9).ToUnixTimeSeconds(),
            iss = appId
        })));

        using var rsa = RSA.Create();
        rsa.ImportFromPem(privateKeyPem);
        var signature = rsa.SignData(Encoding.ASCII.GetBytes($"{header}.{payload}"), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        return $"{header}.{payload}.{Base64Url(signature)}";
    }

    public async Task<GitResult<string>> GetInstallationTokenAsync(int connectionId, string appId, string privateKeyPem, Uri apiBase, HttpClient http, string? owner)
    {
        var ownerKey = string.IsNullOrWhiteSpace(owner) ? "*" : owner.Trim().ToLowerInvariant();
        var cacheKey = $"{connectionId}:{ownerKey}";
        if (Tokens.TryGetValue(cacheKey, out var cached) && cached.ExpiresOn > DateTime.UtcNow.AddMinutes(5))
            return GitResult<string>.Ok(cached.Token);

        var jwt = CreateAppJwt(appId, privateKeyPem);
        var installation = await ResolveInstallationAsync(connectionId, ownerKey, owner, apiBase, http, jwt);
        if (!installation.Success)
            return GitResult<string>.Fail(installation.Error!, installation.StatusCode);

        using var request = Request(HttpMethod.Post, new Uri(apiBase, $"app/installations/{installation.Data}/access_tokens"), jwt);
        using var response = await http.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            Installations.TryRemove(cacheKey, out _);
            return GitResult<string>.Fail($"GitHub refused a GitHub App installation token ({(int)response.StatusCode}). Check that the app is still installed and has not been suspended.", (int)response.StatusCode);
        }

        var json = JObject.Parse(await response.Content.ReadAsStringAsync());
        var token = json["token"]?.ToString();
        if (string.IsNullOrWhiteSpace(token))
            return GitResult<string>.Fail("GitHub returned an empty GitHub App installation token.");

        var expires = json["expires_at"]?.ToObject<DateTime?>()?.ToUniversalTime() ?? DateTime.UtcNow.AddMinutes(55);
        Tokens[cacheKey] = (token, expires);
        return GitResult<string>.Ok(token);
    }

    public void Forget(int connectionId)
    {
        foreach (var key in Tokens.Keys.Where(k => k.StartsWith($"{connectionId}:", StringComparison.Ordinal)))
            Tokens.TryRemove(key, out _);
        foreach (var key in Installations.Keys.Where(k => k.StartsWith($"{connectionId}:", StringComparison.Ordinal)))
            Installations.TryRemove(key, out _);
    }

    #endregion

    #region Utilities

    private static async Task<GitResult<long>> ResolveInstallationAsync(int connectionId, string ownerKey, string? owner, Uri apiBase, HttpClient http, string jwt)
    {
        var cacheKey = $"{connectionId}:{ownerKey}";
        if (Installations.TryGetValue(cacheKey, out var known))
            return GitResult<long>.Ok(known);

        long? id = null;
        var status = 0;
        if (!string.IsNullOrWhiteSpace(owner))
        {
            foreach (var path in new[] { $"orgs/{Uri.EscapeDataString(owner)}/installation", $"users/{Uri.EscapeDataString(owner)}/installation" })
            {
                using var request = Request(HttpMethod.Get, new Uri(apiBase, path), jwt);
                using var response = await http.SendAsync(request);
                status = (int)response.StatusCode;
                if (status == 401)
                    break;
                if (!response.IsSuccessStatusCode)
                    continue;

                id = JObject.Parse(await response.Content.ReadAsStringAsync())["id"]?.Value<long?>();
                if (id != null)
                    break;
            }
        }
        else
        {
            using var request = Request(HttpMethod.Get, new Uri(apiBase, "app/installations?per_page=1"), jwt);
            using var response = await http.SendAsync(request);
            status = (int)response.StatusCode;
            if (response.IsSuccessStatusCode)
                id = (JArray.Parse(await response.Content.ReadAsStringAsync()).FirstOrDefault() as JObject)?["id"]?.Value<long?>();
        }

        if (id == null)
        {
            if (status == 401)
                return GitResult<long>.Fail("GitHub rejected the GitHub App credentials (401). Check the App ID and private key under Integrations → Git Connections.", status);

            return GitResult<long>.Fail(string.IsNullOrWhiteSpace(owner)
                ? "The GitHub App is not installed on any account yet. On GitHub open the app's settings → Install App, and install it on the account or organization that owns the repositories."
                : $"The GitHub App is not installed on '{owner}'. On GitHub open the app's settings → Install App, and install it on that account or organization.", status);
        }

        Installations[cacheKey] = id.Value;
        return GitResult<long>.Ok(id.Value);
    }

    private static HttpRequestMessage Request(HttpMethod method, Uri uri, string jwt)
    {
        var request = new HttpRequestMessage(method, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        request.Headers.UserAgent.ParseAdd("Taskist-Git-Integration/1.0");
        request.Headers.Accept.ParseAdd("application/vnd.github+json");
        request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
        return request;
    }

    private static string Base64Url(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    #endregion
}
