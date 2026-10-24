using Taskist.Service.Localization;
using Taskist.Service.Masters;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Masters;
using FluentValidation;

namespace Taskist.Web.Helpers.Validators.Masters;

public class TagValidator : AbstractValidator<TagModel>
{
    public TagValidator(ILocalizationService localizationService,
        ITagService tagService)
    {
        RuleFor(r => r.Name)
            .NotEmpty().WithMessage(localizationService.GetResource("TagModel.Name.RequiredMsg"))
            .MaximumLength(100).WithMessage(localizationService.GetResource("TagModel.Name.MaxLengthMsg"))
            .MustAwait(async (model, name) =>
            {
                var existing = await tagService.GetByNameAsync(name);
                return existing == null || existing.Id == model.Id;
            }).WithMessage(localizationService.GetResource("TagModel.Name.UniqueMsg"));
    }
}
