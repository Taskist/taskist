namespace Taskist.Web.Models.Users;

public class TwoFactorSetupModel
{
    #region Properties

    public string QrCodeDataUri { get; set; } = string.Empty;

    public string ManualKey { get; set; } = string.Empty;

    public string AccountName { get; set; } = string.Empty;

    #endregion
}
