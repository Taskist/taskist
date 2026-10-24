using FluentValidation;
using Taskist.Service.Localization;
using Taskist.Service.WorkItems;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.WorkItems;

namespace Taskist.Web.Helpers.Validators.WorkItems;

public class SprintValidator : AbstractValidator<ReleaseModel>
{
	public SprintValidator(ILocalizationService localizationService,
		IReleaseService releaseService)
	{
		RuleFor(r => r.Name)
			.NotEmpty().WithMessage(localizationService.GetResource("ReleaseModel.Name.RequiredMsg"))
			.MaximumLength(100).WithMessage(localizationService.GetResource("ReleaseModel.Name.MaxLengthMsg"))
			.MustAwait(async (x, context) =>
				!await releaseService.NameExistsInScopeAsync(x.Name, x.AllProjects ? [] : x.SelectedProjectIds ?? [], x.Id))
			.WithMessage(localizationService.GetResource("ReleaseModel.Name.UniqueScopeMsg"))
			.When(r => r.Id > 0 || !r.VersionAuto);

		RuleFor(r => r.ReleaseDate)
			.NotEmpty().WithMessage(localizationService.GetResource("ReleaseModel.StartDate.RequiredMsg"))
			.GreaterThan(DateOnly.MinValue).WithMessage(localizationService.GetResource("ReleaseModel.StartDate.RequiredMsg"));

		RuleFor(r => r.SelectedProjectIds)
			.Must(x => x != null && x.Any(id => id > 0))
			.When(r => !r.AllProjects)
			.WithMessage(localizationService.GetResource("ReleaseModel.SelectedProjectIds.RequiredMsg"));

		RuleFor(r => r.Description)
			.MaximumLength(250).WithMessage(localizationService.GetResource("ReleaseModel.Description.MaxLengthMsg"));
	}
}