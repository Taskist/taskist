namespace Taskist.Web.Models.Users;

public class TwoFactorLoginModel
{
    #region Properties

    public string? Code { get; set; }

    public bool UseRecoveryCode { get; set; }

    public bool RememberDevice { get; set; }

    public int RememberDeviceDays { get; set; }

    public string? ErrorMessage { get; set; }

    #endregion
}
