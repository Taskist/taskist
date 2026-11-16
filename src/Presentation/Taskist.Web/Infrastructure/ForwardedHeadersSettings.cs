namespace Taskist.Web.Infrastructure;

public class ForwardedHeadersSettings
{
    #region Properties

    public bool Enabled { get; set; }

    public string[] KnownProxies { get; set; } = [];

    public string[] KnownNetworks { get; set; } = [];

    #endregion
}
