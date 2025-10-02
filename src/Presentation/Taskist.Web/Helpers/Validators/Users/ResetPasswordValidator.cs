using FluentValidation;
using Taskist.Service.Localization;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Users;

namespace Taskist.Web.Helpers.Validators.Users;

public class ResetPasswordValidator : AbstractValidator<ResetPasswordModel>
{
    public ResetPasswordValidator(ILocalizationService localizationService)
    {
        RuleFor(r => r.Password)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("ResetPasswordModel.Password.RequiredMsg"))
            .MaximumLength(100).WithMessageAwait(localizationService.GetResourceAsync("ResetPasswordModel.Password.MaxLengthMsg"));
    }
}