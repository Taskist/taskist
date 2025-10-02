using FluentValidation;
using Taskist.Service.Localization;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Localization;

namespace Taskist.Web.Helpers.Validators.Localization;

public class LanguageValidator : AbstractValidator<LanguageModel>
{
    public LanguageValidator(ILocalizationService localizationService,
        ILanguageService languageService)
    {
        RuleFor(r => r.Name)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("LanguageModel.Name.RequiredMsg"))
            .MaximumLength(100).WithMessageAwait(localizationService.GetResourceAsync("LanguageModel.Name.MaxLengthMsg"))
            .MustAwait(async (x, context) =>
            {
                if (x.Id > 0)
                {
                    var editedEntity = await languageService.GetByNameAsync(x.Name);
                    return editedEntity == null || editedEntity.Id == x.Id;
                }
                var entity = await languageService.GetByNameAsync(x.Name);
                return entity == null;
            }).WithMessageAwait(localizationService.GetResourceAsync("LanguageModel.Name.UniqueMsg"));

        RuleFor(r => r.LanguageCulture)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("LanguageModel.LanguageCulture.RequiredMsg"))
            .MaximumLength(10).WithMessageAwait(localizationService.GetResourceAsync("LanguageModel.LanguageCulture.MaxLengthMsg"))
            .MustAwait(async (x, context) =>
            {
                if (x.Id > 0)
                {
                    var editedEntity = await languageService.GetByCultureAsync(x.LanguageCulture);
                    return editedEntity == null || editedEntity.Id == x.Id;
                }
                var entity = await languageService.GetByCultureAsync(x.LanguageCulture);
                return entity == null;
            }).WithMessageAwait(localizationService.GetResourceAsync("LanguageModel.LanguageCulture.UniqueMsg"));

        RuleFor(r => r.DisplayOrder)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("LanguageModel.DisplayOrder.RequiredMsg"));
    }
}