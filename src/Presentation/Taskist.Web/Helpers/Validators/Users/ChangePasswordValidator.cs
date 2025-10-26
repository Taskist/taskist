using FluentValidation;
using Taskist.Core.Common;
using Taskist.Service.Localization;
using Taskist.Service.Security;
using Taskist.Service.Users;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Users;

namespace Taskist.Web.Helpers.Validators.Users;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordModel>
{
    public ChangePasswordValidator(IUserService userService,
        IWorkContext workContext,
        IEncryptionService encryptionService,
        ILocalizationService localizationService)
    {
        RuleFor(r => r.CurrentPassword)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("ChangePasswordModel.CurrentPassword.RequiredMsg"))
            .MaximumLength(250).WithMessageAwait(localizationService.GetResourceAsync("ChangePasswordModel.CurrentPassword.MaxLengthMsg"))
            .MustAwait(async (x, context) =>
            {
                var user = await workContext.GetCurrentUserAsync();
                var activePassword = await userService.GetCurrentPasswordAsync(user.Id);

                var enteredPassword = encryptionService.CreatePasswordHash(x.CurrentPassword, activePassword.PasswordSalt);
                return activePassword.Password.Equals(enteredPassword);
            }).WithMessageAwait(localizationService.GetResourceAsync("ChangePasswordModel.CurrentPassword.NotMatchMsg"));

        RuleFor(r => r.NewPassword)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("ChangePasswordModel.NewPassword.RequiredMsg"))
            .MaximumLength(50).WithMessageAwait(localizationService.GetResourceAsync("ChangePasswordModel.NewPassword.MaxLengthMsg"));

        RuleFor(r => r.ConfirmPassword)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("ChangePasswordModel.ConfirmPassword.RequiredMsg"))
            .MaximumLength(50).WithMessageAwait(localizationService.GetResourceAsync("ChangePasswordModel.ConfirmPassword.MaxLengthMsg"))
            .Equal(x => x.NewPassword).WithMessageAwait(localizationService.GetResourceAsync("ChangePasswordModel.ConfirmPassword.NotMatchMsg"));
    }
}