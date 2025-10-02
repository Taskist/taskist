using FluentValidation;
using Taskist.Service.Localization;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Localization;

namespace Taskist.Web.Helpers.Validators.Localization;

public class LocaleResourceValidator : AbstractValidator<LocaleResourceModel>
{
    public LocaleResourceValidator(ILocalizationService localizationService)
    {
        RuleFor(r => r.ResourceKey)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("LocaleResourceModel.ResourceKey.RequiredMsg"))
            .MaximumLength(150).WithMessageAwait(localizationService.GetResourceAsync("LocaleResourceModel.ResourceKey.MaxLengthMsg"))
            .MustAwait(async (x, context) =>
            {
                if (x.Id > 0)
                {
                    var editedEntity = await localizationService.GetResourceByKeyAsync(x.LanguageId, x.ResourceKey);
                    return editedEntity == null || editedEntity.Id == x.Id;
                }
                var entity = await localizationService.GetResourceByKeyAsync(x.LanguageId, x.ResourceKey);
                return entity == null;
            }).WithMessageAwait(localizationService.GetResourceAsync("LocaleResourceModel.ResourceKey.UniqueMsg")); ;

        RuleFor(r => r.ResourceValue)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("LocaleResourceModel.ResourceValue.RequiredMsg"))
            .MaximumLength(100).WithMessageAwait(localizationService.GetResourceAsync("LocaleResourceModel.ResourceValue.MaxLengthMsg"));
    }
}