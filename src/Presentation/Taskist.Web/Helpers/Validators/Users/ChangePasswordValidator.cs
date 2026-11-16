using FluentValidation;
using Taskist.Core.Common;
using Taskist.Core.Domain.Settings;
using Taskist.Service.Localization;
using Taskist.Service.Users;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Users;

namespace Taskist.Web.Helpers.Validators.Users;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordModel>
{
    public ChangePasswordValidator(ILocalizationService localizationService,
        IUserService userService,
        IWorkContext workContext,
        SecuritySettings securitySettings)
    {
        RuleFor(r => r.CurrentPassword)
            .NotEmpty().WithMessage(localizationService.GetResource("ChangePasswordModel.CurrentPassword.RequiredMsg"))
            .MaximumLength(100).WithMessage(localizationService.GetResource("ChangePasswordModel.CurrentPassword.MaxLengthMsg"))
            .MustAwait(async (x, context) =>
            {
                var loggedUser = await workContext.GetCurrentUserAsync();
                var valid = await userService.ValidateCurrentPasswordAsync(loggedUser.Id, x.CurrentPassword);
                return valid;
            }).WithMessage(localizationService.GetResource("ChangePasswordModel.CurrentPassword.WrongPassword"));

        RuleFor(r => r.NewPassword)
            .NotEmpty().WithMessage(localizationService.GetResource("ChangePasswordModel.NewPassword.RequiredMsg"))
            .MinimumLength(securitySettings.PasswordMinLength).WithMessage(string.Format(localizationService.GetResource("Validation.PasswordMinLength"), securitySettings.PasswordMinLength))
            .MaximumLength(100).WithMessage(localizationService.GetResource("ChangePasswordModel.NewPassword.MaxLengthMsg"));

        RuleFor(r => r.ConfirmNewPassword)
            .NotEmpty().WithMessage(localizationService.GetResource("ChangePasswordModel.ConfirmNewPassword.RequiredMsg"))
            .MaximumLength(100).WithMessage(localizationService.GetResource("ChangePasswordModel.ConfirmNewPassword.MaxLengthMsg"))
            .Equal(x => x.NewPassword).WithMessage(localizationService.GetResource("ChangePasswordModel.ConfirmNewPassword.NotMatch"));
    }
}