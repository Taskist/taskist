using Taskist.Service.Localization;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Localization;
using FluentValidation;

namespace Taskist.Web.Helpers.Validators.Localization;

public class LanguageValidator : AbstractValidator<LanguageModel>
{
    public LanguageValidator(ILocalizationService localizationService,
        ILanguageService languageService)
    {
        RuleFor(r => r.Name)
            .NotEmpty().WithMessage(localizationService.GetResource("LanguageModel.Name.RequiredMsg"))
            .MaximumLength(100).WithMessage(localizationService.GetResource("LanguageModel.Name.MaxLengthMsg"))
            .MustAwait(async (x, context) =>
            {
                if (x.Id > 0)
                {
                    var editedEntity = await languageService.GetByNameAsync(x.Name);
                    return editedEntity == null || editedEntity.Id == x.Id;
                }
                var entity = await languageService.GetByNameAsync(x.Name);
                return entity == null;
            }).WithMessage(localizationService.GetResource("LanguageModel.Name.UniqueMsg"));

        RuleFor(r => r.LanguageCulture)
            .NotEmpty().WithMessage(localizationService.GetResource("LanguageModel.LanguageCulture.RequiredMsg"))
            .MaximumLength(10).WithMessage(localizationService.GetResource("LanguageModel.LanguageCulture.MaxLengthMsg"))
            .MustAwait(async (x, context) =>
            {
                if (x.Id > 0)
                {
                    var editedEntity = await languageService.GetByCultureAsync(x.LanguageCulture);
                    return editedEntity == null || editedEntity.Id == x.Id;
                }
                var entity = await languageService.GetByCultureAsync(x.LanguageCulture);
                return entity == null;
            }).WithMessage(localizationService.GetResource("LanguageModel.LanguageCulture.UniqueMsg"));

        RuleFor(r => r.DisplayOrder)
            .NotEmpty().WithMessage(localizationService.GetResource("LanguageModel.DisplayOrder.RequiredMsg"));
    }
}