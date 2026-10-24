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
            .NotEmpty().WithMessage(localizationService.GetResource("UserModel.UserNumber.RequiredMsg"))
            .MaximumLength(20).WithMessage(localizationService.GetResource("UserModel.UserNumber.MaxLengthMsg"));

        RuleFor(r => r.FirstName)
            .NotEmpty().WithMessage(localizationService.GetResource("UserModel.FirstName.RequiredMsg"))
            .MaximumLength(50).WithMessage(localizationService.GetResource("UserModel.FirstName.MaxLengthMsg"));

        RuleFor(r => r.LastName)
            .NotEmpty().WithMessage(localizationService.GetResource("UserModel.LastName.RequiredMsg"))
            .MaximumLength(50).WithMessage(localizationService.GetResource("UserModel.LastName.MaxLengthMsg"));

        RuleFor(r => r.Email)
            .NotEmpty().WithMessage(localizationService.GetResource("UserModel.Email.RequiredMsg"))
            .MaximumLength(250).WithMessage(localizationService.GetResource("UserModel.Email.MaxLengthMsg"))
            .EmailAddress().WithMessage(localizationService.GetResource("UserModel.Email.InvalidEmailMsg"))
            .MustAwait(async (x, context) =>
            {
                if (x.Id > 0)
                {
                    var editedEntity = await userService.GetByEmailAsync(x.Email);
                    return editedEntity == null || editedEntity.Email == x.Email;
                }
                var entity = await userService.GetByEmailAsync(x.Email);
                return entity == null;
            }).WithMessage(localizationService.GetResource("UserModel.Email.UniqueMsg"));

        RuleFor(r => r.MobileNumber)
            .NotEmpty().WithMessage(localizationService.GetResource("UserModel.MobileNumber.RequiredMsg"))
            .MaximumLength(20).WithMessage(localizationService.GetResource("UserModel.MobileNumber.InvalidMsg"));

        RuleFor(r => r.GenderId)
            .NotEmpty().WithMessage(localizationService.GetResource("UserModel.Gender.RequiredMsg"))
            .GreaterThan(0).WithMessage(localizationService.GetResource("UserModel.Gender.RequiredMsg"));

        RuleFor(r => r.DateOfBirth)
            .NotEmpty().WithMessage(localizationService.GetResource("UserModel.DateOfBirth.RequiredMsg"))
            .GreaterThan(DateOnly.MinValue).WithMessage(localizationService.GetResource("UserModel.DateOfBirth.InvalidMsg"));

        RuleFor(r => r.DateOfJoin)
            .NotEmpty().WithMessage(localizationService.GetResource("UserModel.DateOfJoin.RequiredMsg"))
            .GreaterThan(DateOnly.MinValue).WithMessage(localizationService.GetResource("UserModel.DateOfJoin.InvalidMsg"));

        RuleFor(r => r.SelectedRoleIds)
            .NotEmpty().WithMessage(localizationService.GetResource("UserModel.Role.RequiredMsg"));

        RuleFor(r => r.Status)
            .NotEmpty().WithMessage(localizationService.GetResource("UserModel.Status.RequiredMsg"))
            .GreaterThan(0).WithMessage(localizationService.GetResource("UserModel.Status.RequiredMsg"));
    }
}