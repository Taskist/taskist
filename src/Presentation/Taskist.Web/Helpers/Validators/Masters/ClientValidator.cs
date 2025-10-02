using FluentValidation;
using Taskist.Service.Localization;
using Taskist.Service.Masters;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Masters;

namespace Taskist.Web.Helpers.Validators.Masters;

public class ClientValidator : AbstractValidator<ClientModel>
{
    public ClientValidator(ILocalizationService localizationService,
        IClientService clientService)
    {
        RuleFor(r => r.Name)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("ClientModel.Name.RequiredMsg"))
            .MaximumLength(100).WithMessageAwait(localizationService.GetResourceAsync("ClientModel.Name.MaxLengthMsg"))
            .MustAwait(async (x, context) =>
            {
                if (x.Id > 0)
                {
                    var editedEntity = await clientService.GetByNameAsync(x.Name);
                    return editedEntity == null || editedEntity.Id == x.Id;
                }
                var entity = await clientService.GetByNameAsync(x.Name);
                return entity == null;
            }).WithMessageAwait(localizationService.GetResourceAsync("ClientModel.Name.UniqueMsg"));

        RuleFor(r => r.Description)
            .MaximumLength(250).WithMessageAwait(localizationService.GetResourceAsync("ClientModel.Description.MaxLengthMsg"));

        RuleFor(r => r.ContactPerson)
            .MaximumLength(100).WithMessageAwait(localizationService.GetResourceAsync("ClientModel.ContactPerson.MaxLengthMsg"));

        RuleFor(r => r.PhoneNumber)
            .MaximumLength(100).WithMessageAwait(localizationService.GetResourceAsync("ClientModel.PhoneNumber.MaxLengthMsg"));

        RuleFor(r => r.Email)
            .EmailAddress().WithMessageAwait(localizationService.GetResourceAsync("ClientModel.Email.InvalidEmailMsg"))
            .MaximumLength(250).WithMessageAwait(localizationService.GetResourceAsync("ClientModel.Email.MaxLengthMsg"));

        RuleFor(r => r.Website)
            .MaximumLength(750).WithMessageAwait(localizationService.GetResourceAsync("ClientModel.Website.MaxLengthMsg"));
    }
}