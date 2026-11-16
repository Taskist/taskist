using Taskist.Core.Domain.Settings;
using Taskist.Service.Localization;
using Taskist.Web.Models.Users;
using FluentValidation;

namespace Taskist.Web.Helpers.Validators.Users;

public class ResetPasswordValidator : AbstractValidator<ResetPasswordModel>
{
    public ResetPasswordValidator(ILocalizationService localizationService, SecuritySettings securitySettings)
    {
        RuleFor(r => r.Password)
            .NotEmpty().WithMessage(localizationService.GetResource("ResetPasswordModel.Password.RequiredMsg"))
            .MinimumLength(securitySettings.PasswordMinLength).WithMessage(string.Format(localizationService.GetResource("Validation.PasswordMinLength"), securitySettings.PasswordMinLength))
            .MaximumLength(100).WithMessage(localizationService.GetResource("ResetPasswordModel.Password.MaxLengthMsg"));
    }
}