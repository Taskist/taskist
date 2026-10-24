using System.Net;
using System.Net.Sockets;

namespace Taskist.Service.Git;

public static class GitHttp
{
    #region Fields

    public const string ClientName = "taskist-git";
    public const string LocalClientName = "taskist-git-local";
    public const long MaxResponseBytes = 8 * 1024 * 1024;
    public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

    #endregion

    #region Methods

    public static SocketsHttpHandler CreateHandler(bool allowLocal)
    {
        return new SocketsHttpHandler
        {
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.All,
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            ConnectTimeout = TimeSpan.FromSeconds(10),
            ConnectCallback = (context, cancellationToken) => ConnectAsync(context, allowLocal, cancellationToken)
        };
    }

    public static string? ValidateBaseUrl(string? value, bool allowInsecureHttp)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Enter the URL.";

        if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri))
            return "Enter a full URL, for example https://gitlab.example.com.";

        if (uri.Scheme != Uri.UriSchemeHttps && !(allowInsecureHttp && uri.Scheme == Uri.UriSchemeHttp))
            return "Use an https:// URL. Plain http is only allowed when 'Allow insecure HTTP (development only)' is ticked.";

        if (!string.IsNullOrEmpty(uri.UserInfo))
            return "Do not put a user name or password in the URL.";

        if (!string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment))
            return "The URL must not contain a query string or fragment.";

        return null;
    }

    public static bool IsBlockedAddress(IPAddress address, bool allowLocal)
    {
        if (address.IsIPv4MappedToIPv6)
            address = address.MapToIPv4();

        if (IPAddress.IsLoopback(address))
            return !allowLocal;

        if (address.Equals(IPAddress.Any) || address.Equals(IPAddress.IPv6Any) || address.Equals(IPAddress.Broadcast))
            return true;

        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            var bytes = address.GetAddressBytes();
            if (bytes[0] == 169 && bytes[1] == 254)
                return true;
            if (bytes[0] == 0 || bytes[0] >= 224)
                return true;
            return false;
        }

        if (address.IsIPv6LinkLocal || address.IsIPv6Multicast || address.IsIPv6SiteLocal)
            return true;

        var v6 = address.GetAddressBytes();
        return v6[0] == 0xfd && v6[1] == 0x00 && v6[2] == 0x0e && v6[3] == 0xc2;
    }

    #endregion

    #region Utilities

    private static async ValueTask<Stream> ConnectAsync(SocketsHttpConnectionContext context, bool allowLocal, CancellationToken cancellationToken)
    {
        var host = context.DnsEndPoint.Host;
        var addresses = IPAddress.TryParse(host, out var literal)
            ? [literal]
            : await Dns.GetHostAddressesAsync(host, cancellationToken);

        if (addresses.Length == 0)
            throw new HttpRequestException($"Could not resolve {host}.");

        if (addresses.Any(a => IsBlockedAddress(a, allowLocal)))
            throw new HttpRequestException($"Connections to {host} are blocked because it resolves to a loopback, link-local or metadata address.");

        var socket = new Socket(SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
        try
        {
            await socket.ConnectAsync(addresses, context.DnsEndPoint.Port, cancellationToken);
            return new NetworkStream(socket, ownsSocket: true);
        }
        catch
        {
            socket.Dispose();
            throw;
        }
    }

    #endregion
}
