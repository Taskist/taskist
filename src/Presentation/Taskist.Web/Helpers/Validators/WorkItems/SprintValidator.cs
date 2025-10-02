using FluentValidation;
using Taskist.Core.Extensions;
using Taskist.Service.Localization;
using Taskist.Service.WorkItems;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.WorkItems;

namespace Taskist.Web.Helpers.Validators.WorkItems;

public class SprintValidator : AbstractValidator<SprintModel>
{
    public SprintValidator(ILocalizationService localizationService,
        ISprintService sprintService)
    {
        RuleFor(r => r.Name)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("SprintModel.Name.RequiredMsg"))
            .MaximumLength(100).WithMessageAwait(localizationService.GetResourceAsync("SprintModel.Name.MaxLengthMsg"))
            .MustAwait(async (x, context) =>
            {
                if (x.Id > 0)
                {
                    var editedEntity = await sprintService.GetByNameAsync(x.Name);
                    return editedEntity == null || editedEntity.Id == x.Id;
                }
                var entity = await sprintService.GetByNameAsync(x.Name);
                return entity == null;
            }).WithMessageAwait(localizationService.GetResourceAsync("SprintModel.Name.UniqueMsg"));

        RuleFor(r => r.ProjectId)
               .NotNull().WithMessageAwait(localizationService.GetResourceAsync("SprintModel.Project.RequiredMsg"))
               .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("SprintModel.Project.RequiredMsg"))
               .GreaterThan(0).WithMessageAwait(localizationService.GetResourceAsync("SprintModel.Project.RequiredMsg"));

        RuleFor(r => r.StartDate)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("SprintModel.StartDate.RequiredMsg"))
            .GreaterThan(DateOnly.MinValue).WithMessageAwait(localizationService.GetResourceAsync("SprintModel.StartDate.RequiredMsg"))
            .MustAwait(async (x, context) =>
            {
                if (x.Id > 0)
                {
                    var editedEntity = await sprintService.GetSprintBetweenDateAsync(x.ProjectId, x.StartDate.ToDateOnly(), x.EndDate.ToDateOnly());
                    return editedEntity == null || editedEntity.Any(y => y.Id == x.Id);
                }
                var entity = await sprintService.GetSprintBetweenDateAsync(x.ProjectId, x.StartDate.ToDateOnly(), x.EndDate.ToDateOnly());
                return !entity.Any();
            }).WithMessageAwait(localizationService.GetResourceAsync("SprintModel.StartDate.UniqueMsg"));

        RuleFor(r => r.EndDate)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("SprintModel.EndDate.RequiredMsg"))
            .GreaterThan(DateOnly.MinValue).WithMessageAwait(localizationService.GetResourceAsync("SprintModel.EndDate.RequiredMsg"))
            .GreaterThan(x => x.StartDate).WithMessageAwait(localizationService.GetResourceAsync("SprintModel.EndDate.RequiredMsg"))
            .MustAwait(async (x, context) =>
            {
                if (x.Id > 0)
                {
                    var editedEntity = await sprintService.GetSprintBetweenDateAsync(x.ProjectId, x.StartDate.ToDateOnly(), x.EndDate.ToDateOnly());
                    return editedEntity == null || editedEntity.Any(y => y.Id == x.Id);
                }
                var entity = await sprintService.GetSprintBetweenDateAsync(x.ProjectId, x.StartDate.ToDateOnly(), x.EndDate.ToDateOnly());
                return !entity.Any();
            }).WithMessageAwait(localizationService.GetResourceAsync("SprintModel.EndDate.UniqueMsg"));

        RuleFor(r => r.Description)
            .MaximumLength(250).WithMessageAwait(localizationService.GetResourceAsync("SprintModel.Description.MaxLengthMsg"));
    }
}