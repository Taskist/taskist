using Microsoft.AspNetCore.Mvc.Rendering;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.Masters;

public class GitConnectionModel : BaseModel
{
    [LocalizedDisplayName("GitConnectionModel.Name")]
    public string? Name { get; set; }

    [LocalizedDisplayName("GitConnectionModel.ProviderType")]
    public int ProviderType { get; set; } = 2;

    [LocalizedDisplayName("GitConnectionModel.BaseUrl")]
    public string? BaseUrl { get; set; }

    [LocalizedDisplayName("GitConnectionModel.ApiUrl")]
    public string? ApiUrl { get; set; }

    [LocalizedDisplayName("GitConnectionModel.AuthMode")]
    public int AuthMode { get; set; } = 1;

    [LocalizedDisplayName("GitConnectionModel.ServiceToken")]
    public string? ServiceToken { get; set; }

    public bool HasServiceToken { get; set; }

    [LocalizedDisplayName("GitConnectionModel.AppId")]
    public string? AppId { get; set; }

    [LocalizedDisplayName("GitConnectionModel.AppPrivateKey")]
    public string? AppPrivateKey { get; set; }

    public bool HasAppPrivateKey { get; set; }

    [LocalizedDisplayName("GitConnectionModel.OAuthClientId")]
    public string? OAuthClientId { get; set; }

    [LocalizedDisplayName("GitConnectionModel.OAuthClientSecret")]
    public string? OAuthClientSecret { get; set; }

    public bool HasOAuthClientSecret { get; set; }

    public bool ClearOAuth { get; set; }

    [LocalizedDisplayName("GitConnectionModel.AllowInsecureHttp")]
    public bool AllowInsecureHttp { get; set; }

    [LocalizedDisplayName("GitConnectionModel.IsDefault")]
    public bool IsDefault { get; set; }

    [LocalizedDisplayName("GitConnectionModel.Active")]
    public bool Active { get; set; } = true;

    public string? ServiceAccountUserName { get; set; }

    public string? OAuthCallbackUrl { get; set; }

    public IList<SelectListItem> AvailableProviders { get; set; } = [];

    public IList<SelectListItem> AvailableAuthModes { get; set; } = [];
}
