using Taskist.Service.Localization;
using Taskist.Service.Masters;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Masters;
using FluentValidation;

namespace Taskist.Web.Helpers.Validators.Masters;

public class ReporterValidator : AbstractValidator<ReporterModel>
{
    public ReporterValidator(ILocalizationService localizationService,
        IReporterService reporterService)
    {
        RuleFor(r => r.Name)
            .NotEmpty().WithMessage(localizationService.GetResource("ReporterModel.Name.RequiredMsg"))
            .MaximumLength(100).WithMessage(localizationService.GetResource("ReporterModel.Name.MaxLengthMsg"))
            .MustAwait(async (x, context) =>
            {
                if (x.Id > 0)
                {
                    var editedEntity = await reporterService.GetByNameAsync(x.Name);
                    return editedEntity == null || editedEntity.Id == x.Id;
                }
                var entity = await reporterService.GetByNameAsync(x.Name);
                return entity == null;
            }).WithMessage(localizationService.GetResource("ReporterModel.Name.UniqueMsg"));

        RuleFor(r => r.Description)
            .MaximumLength(250).WithMessage(localizationService.GetResource("ReporterModel.Description.MaxLengthMsg"));
    }
}