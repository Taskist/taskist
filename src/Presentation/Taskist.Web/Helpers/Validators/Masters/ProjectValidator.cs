using Taskist.Service.Localization;
using Taskist.Service.Masters;
using Taskist.Service.WorkItems;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Masters;
using FluentValidation;

namespace Taskist.Web.Helpers.Validators.Masters;

public class ProjectValidator : AbstractValidator<ProjectModel>
{
    public ProjectValidator(ILocalizationService localizationService,
        IProjectService projectService)
    {
        RuleFor(r => r.Name)
            .NotEmpty().WithMessage(localizationService.GetResource("ProjectModel.Name.RequiredMsg"))
            .MaximumLength(100).WithMessage(localizationService.GetResource("ProjectModel.Name.MaxLengthMsg"))
            .MustAwait(async (x, context) =>
            {
                if (x.Id > 0)
                {
                    var editedEntity = await projectService.GetByNameAsync(x.Name);
                    return editedEntity == null || editedEntity.Id == x.Id;
                }
                var entity = await projectService.GetByNameAsync(x.Name);
                return entity == null;
            }).WithMessage(localizationService.GetResource("ProjectModel.Name.UniqueMsg"));

        RuleFor(r => r.ReleaseVersionFormat)
            .Must(ReleaseVersionTemplate.IsValid)
            .When(r => !string.IsNullOrWhiteSpace(r.ReleaseVersionFormat))
            .WithMessage(localizationService.GetResource("ProjectModel.ReleaseVersionFormat.InvalidMsg"));

        RuleFor(r => r.Description)
            .MaximumLength(250).WithMessage(localizationService.GetResource("ProjectModel.Description.MaxLengthMsg"));
    }
}