using Taskist.Service.Localization;
using Taskist.Web.Models.Users;
using FluentValidation;

namespace Taskist.Web.Helpers.Validators.Users;

public class ProfileValidator : AbstractValidator<ProfileModel>
{
	public ProfileValidator(ILocalizationService localizationService)
	{
		RuleFor(r => r.FirstName)
			.NotEmpty().WithMessage(localizationService.GetResource("Profile.FirstName.RequiredMsg"))
			.MaximumLength(50).WithMessage(localizationService.GetResource("Profile.FirstName.MaxLengthMsg"));

		RuleFor(r => r.LastName)
			.NotEmpty().WithMessage(localizationService.GetResource("Profile.LastName.RequiredMsg"))
			.MaximumLength(50).WithMessage(localizationService.GetResource("Profile.LastName.MaxLengthMsg"));

		RuleFor(r => r.MobileNumber)
			.NotEmpty().WithMessage(localizationService.GetResource("Profile.Mobile.RequiredMsg"))
			.Matches(@"^\d{10}$").WithMessage(localizationService.GetResource("Profile.Mobile.InvalidMsg"));
	}
}
