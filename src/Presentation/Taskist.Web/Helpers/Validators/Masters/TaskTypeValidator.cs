using FluentValidation;
using Taskist.Service.Localization;
using Taskist.Service.Masters;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Masters;

namespace Taskist.Web.Helpers.Validators.Masters;

public class TaskTypeValidator : AbstractValidator<TaskTypeModel>
{
    public TaskTypeValidator(ILocalizationService localizationService,
        ITaskTypeService severityService)
    {
        RuleFor(r => r.Name)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("TaskTypeModel.Name.RequiredMsg"))
            .MaximumLength(100).WithMessageAwait(localizationService.GetResourceAsync("TaskTypeModel.Name.MaxLengthMsg"))
            .MustAwait(async (x, context) =>
            {
                if (x.Id > 0)
                {
                    var editedEntity = await severityService.GetByNameAsync(x.Name);
                    return editedEntity == null || editedEntity.Id == x.Id;
                }
                var entity = await severityService.GetByNameAsync(x.Name);
                return entity == null;
            }).WithMessageAwait(localizationService.GetResourceAsync("TaskTypeModel.Name.UniqueMsg"));

        RuleFor(r => r.Description)
            .MaximumLength(250).WithMessageAwait(localizationService.GetResourceAsync("TaskTypeModel.Description.MaxLengthMsg"));

        RuleFor(r => r.GroupId)
           .GreaterThan(0).WithMessageAwait(localizationService.GetResourceAsync("TaskTypeModel.Group.RequiredMsg"));

        RuleFor(r => r.TextColor)
            .MaximumLength(20).WithMessageAwait(localizationService.GetResourceAsync("TaskTypeModel.TextColor.MaxLengthMsg"));

        RuleFor(r => r.BackgroundColor)
            .MaximumLength(20).WithMessageAwait(localizationService.GetResourceAsync("TaskTypeModel.BackgroundColor.MaxLengthMsg"));

        RuleFor(r => r.IconClass)
            .MaximumLength(50).WithMessageAwait(localizationService.GetResourceAsync("TaskTypeModel.IconClass.MaxLengthMsg"));
    }
}