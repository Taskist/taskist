using FluentValidation;
using Taskist.Service.Localization;
using Taskist.Service.Users;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Users;

namespace Taskist.Web.Helpers.Validators.Users;

public class UserRoleValidator : AbstractValidator<UserRoleModel>
{
    public UserRoleValidator(ILocalizationService localizationService,
    IUserService userService)
    {
        RuleFor(r => r.Name)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("UserRoleModel.Name.RequiredMsg"))
            .MaximumLength(50).WithMessageAwait(localizationService.GetResourceAsync("UserRoleModel.Name.MaxLengthMsg"))
            .MustAwait(async (x, context) =>
            {
                if (x.Id > 0)
                {
                    var editedEntity = await userService.GetUserRoleByNameAsync(x.Name);
                    return (editedEntity != null) && (editedEntity.Name == x.Name);
                }

                var entity = await userService.GetUserRoleByNameAsync(x.Name);
                return entity == null;
            }).WithMessageAwait(localizationService.GetResourceAsync("UserRoleModel.Name.UniqueMsg"));

        RuleFor(r => r.SystemName)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("UserRoleModel.SystemName.RequiredMsg"))
            .MaximumLength(50).WithMessageAwait(localizationService.GetResourceAsync("UserRoleModel.SystemName.MaxLengthMsg"))
            .MustAwait(async (x, context) =>
            {
                if (x.Id > 0)
                {
                    var editedEntity = await userService.GetUserRoleBySystemNameAsync(x.Name);
                    return (editedEntity != null) && (editedEntity.SystemName == x.SystemName);
                }
                var entity = await userService.GetUserRoleBySystemNameAsync(x.SystemName);
                return entity == null;
            }).WithMessageAwait(localizationService.GetResourceAsync("UserRoleModel.SystemName.UniqueMsg"));

        RuleFor(r => r.Description)
            .MaximumLength(250).WithMessageAwait(localizationService.GetResourceAsync("UserRoleModel.Description.MaxLengthMsg"));
    }
}