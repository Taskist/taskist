namespace Taskist.Web.Infrastructure;

public class HostSecurityOptions
{
    #region Fields

    public const string SectionName = "Security";

    #endregion

    #region Properties

    public bool RequireHttpsCookies { get; set; } = true;

    public ForwardedHeadersSettings ForwardedHeaders { get; set; } = new();

    #endregion
}
