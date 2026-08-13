using FluentValidation;
using Taskist.Service.Localization;
using Taskist.Service.Users;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Users;

namespace Taskist.Web.Helpers.Validators.Users;

public class ProfileDetailValidator : AbstractValidator<ProfileDetailModel>
{
    public ProfileDetailValidator(IUserService userService,
        ILocalizationService localizationService)
    {
        RuleFor(r => r.FirstName)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("ProfileDetailModel.FirstName.RequiredMsg"))
            .MaximumLength(50).WithMessageAwait(localizationService.GetResourceAsync("ProfileDetailModel.FirstName.MaxLengthMsg"));

        RuleFor(r => r.LastName)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("ProfileDetailModel.LastName.RequiredMsg"))
            .MaximumLength(50).WithMessageAwait(localizationService.GetResourceAsync("ProfileDetailModel.LastName.MaxLengthMsg"));

        RuleFor(r => r.Email)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("ProfileDetailModel.Email.RequiredMsg"))
            .MaximumLength(250).WithMessageAwait(localizationService.GetResourceAsync("ProfileDetailModel.Email.MaxLengthMsg"))
            .EmailAddress().WithMessageAwait(localizationService.GetResourceAsync("ProfileDetailModel.Email.InvalidMsg"))
            .MustAwait(async (x, context) =>
            {
                if (x.Id > 0)
                {
                    var editedEntity = await userService.GetByEmailAsync(x.Email);
                    return (editedEntity != null) && (editedEntity.Email == x.Email);
                }
                var entity = await userService.GetByEmailAsync(x.Email);
                return entity == null;
            }).WithMessageAwait(localizationService.GetResourceAsync("ProfileDetailModel.Email.UniqueMsg"));

        RuleFor(r => r.GenderId)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("ProfileDetailModel.Gender.RequiredMsg"))
            .GreaterThan(0).WithMessageAwait(localizationService.GetResourceAsync("ProfileDetailModel.Gender.RequiredMsg"));

        RuleFor(r => r.LanguageId)
           .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("ProfileDetailModel.Language.RequiredMsg"))
           .GreaterThan(0).WithMessageAwait(localizationService.GetResourceAsync("ProfileDetailModel.Language.RequiredMsg"));
    }
}
