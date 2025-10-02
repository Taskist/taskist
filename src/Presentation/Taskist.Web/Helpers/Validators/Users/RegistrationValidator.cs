using FluentValidation;
using Taskist.Service.Localization;
using Taskist.Service.Users;
using Taskist.Web.Models.Users;

namespace Taskist.Web.Helpers.Validators.Users;

public class RegistrationValidator : AbstractValidator<UserRegistrationModel>
{
    public RegistrationValidator(ILocalizationService localizationService,
        IUserService userService)
    {
        RuleFor(r => r.User).SetValidator(new UserValidator(localizationService, userService));
    }
}