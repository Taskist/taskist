using Taskist.Service.Localization;
using Taskist.Service.Masters;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Masters;
using FluentValidation;

namespace Taskist.Web.Helpers.Validators.Masters;

public class ModuleValidator : AbstractValidator<ModuleModel>
{
    public ModuleValidator(ILocalizationService localizationService,
        IModuleService moduleService)
    {
        RuleFor(r => r.Name)
            .NotEmpty().WithMessage(localizationService.GetResource("ModuleModel.Name.RequiredMsg"))
            .MaximumLength(100).WithMessage(localizationService.GetResource("ModuleModel.Name.MaxLengthMsg"))
            .MustAwait(async (x, context) =>
            {
                if (x.Id > 0)
                {
                    var editedEntity = await moduleService.GetByNameAsync(x.Name);
                    return editedEntity == null || editedEntity.Id == x.Id;
                }
                var entity = await moduleService.GetByNameAsync(x.Name);
                return entity == null;
            }).WithMessage(localizationService.GetResource("ModuleModel.Name.UniqueMsg"));

        RuleFor(r => r.Description)
            .MaximumLength(250).WithMessage(localizationService.GetResource("ModuleModel.Description.MaxLengthMsg"));
    }
}