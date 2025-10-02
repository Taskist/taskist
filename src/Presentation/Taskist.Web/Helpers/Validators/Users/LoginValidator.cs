using Taskist.Service.Localization;
using FluentValidation;
using Taskist.Web.Models.Users;

namespace Taskist.Web.Helpers.Validators.Users;

public class LoginValidator : AbstractValidator<LoginModel>
{
    public LoginValidator(ILocalizationService localizationService)
    {
        RuleFor(r => r.Email)
            .NotEmpty().WithMessage("Username is required")
            .EmailAddress().WithMessage("Invalid email address");

        RuleFor(r => r.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}