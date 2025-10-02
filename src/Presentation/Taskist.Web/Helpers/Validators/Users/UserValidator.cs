using FluentValidation;
using Taskist.Service.Localization;
using Taskist.Service.Users;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Users;

namespace Taskist.Web.Helpers.Validators.Users;

public class UserValidator : AbstractValidator<UserModel>
{
    public UserValidator(ILocalizationService localizationService,
        IUserService userService)
    {
        RuleFor(r => r.UserNumber)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("UserModel.UserNumber.RequiredMsg"))
            .MaximumLength(20).WithMessageAwait(localizationService.GetResourceAsync("UserModel.UserNumber.MaxLengthMsg"));

        RuleFor(r => r.FirstName)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("UserModel.FirstName.RequiredMsg"))
            .MaximumLength(50).WithMessageAwait(localizationService.GetResourceAsync("UserModel.FirstName.MaxLengthMsg"));

        RuleFor(r => r.LastName)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("UserModel.LastName.RequiredMsg"))
            .MaximumLength(50).WithMessageAwait(localizationService.GetResourceAsync("UserModel.LastName.MaxLengthMsg"));

        RuleFor(r => r.Email)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("UserModel.Email.RequiredMsg"))
            .MaximumLength(250).WithMessageAwait(localizationService.GetResourceAsync("UserModel.Email.MaxLengthMsg"))
            .EmailAddress().WithMessageAwait(localizationService.GetResourceAsync("UserModel.Email.InvalidEmailMsg"))
            .MustAwait(async (x, context) =>
            {
                if (x.Id > 0)
                {
                    var editedEntity = await userService.GetByEmailAsync(x.Email);
                    return (editedEntity != null) && (editedEntity.Email == x.Email);
                }
                var entity = await userService.GetByEmailAsync(x.Email);
                return entity == null;
            }).WithMessageAwait(localizationService.GetResourceAsync("UserModel.Email.UniqueMsg"));

        RuleFor(r => r.GenderId)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("UserModel.Gender.RequiredMsg"))
            .GreaterThan(0).WithMessageAwait(localizationService.GetResourceAsync("UserModel.Gender.RequiredMsg"));

        RuleFor(r => r.SelectedRoleIds)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("UserModel.Role.RequiredMsg"));

        RuleFor(r => r.Status)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("UserModel.Status.RequiredMsg"))
            .GreaterThan(0).WithMessageAwait(localizationService.GetResourceAsync("UserModel.Status.RequiredMsg"));
    }
}