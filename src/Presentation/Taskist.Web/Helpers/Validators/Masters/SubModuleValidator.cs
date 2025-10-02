using FluentValidation;
using Taskist.Service.Localization;
using Taskist.Service.Masters;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Masters;

namespace Taskist.Web.Helpers.Validators.Masters;

public class SubModuleValidator : AbstractValidator<SubModuleModel>
{
    public SubModuleValidator(ILocalizationService localizationService,
        ISubModuleService subModuleService)
    {
        RuleFor(r => r.Name)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("SubModuleModel.Name.RequiredMsg"))
            .MaximumLength(100).WithMessageAwait(localizationService.GetResourceAsync("SubModuleModel.Name.MaxLengthMsg"))
            .MustAwait(async (x, context) =>
            {
                if (x.Id > 0)
                {
                    var editedEntity = await subModuleService.GetByNameAsync(x.Name);
                    return editedEntity == null || editedEntity.Id == x.Id;
                }
                var entity = await subModuleService.GetByNameAsync(x.Name);
                return entity == null;
            }).WithMessageAwait(localizationService.GetResourceAsync("SubModuleModel.Name.UniqueMsg"));

        RuleFor(r => r.Description)
            .MaximumLength(250).WithMessageAwait(localizationService.GetResourceAsync("SubModuleModel.Description.MaxLengthMsg"));
    }
}