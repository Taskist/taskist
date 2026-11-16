using Taskist.Service.Localization;
using Taskist.Web.Models.Masters;
using FluentValidation;

namespace Taskist.Web.Helpers.Validators.Masters;

public class ProjectRepositoryValidator : AbstractValidator<ProjectRepositoryModel>
{
    public ProjectRepositoryValidator(ILocalizationService localizationService)
    {
        RuleFor(r => r.GitConnectionId)
            .GreaterThan(0).WithMessage(localizationService.GetResource("ProjectRepositoryModel.GitConnectionId.RequiredMsg"));

        RuleFor(r => r.RepoRef)
            .NotEmpty().WithMessage(localizationService.GetResource("ProjectRepositoryModel.RepoRef.RequiredMsg"))
            .MaximumLength(255);

        RuleFor(r => r.IssueImportLabel)
            .MaximumLength(100);

        RuleFor(r => r.IssueImportLimitPerHour)
            .InclusiveBetween(1, 1000);

        RuleFor(r => r.IssueSyncMode)
            .InclusiveBetween(0, 3);
    }
}
