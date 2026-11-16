using Taskist.Service.Localization;
using Taskist.Service.Masters;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Masters;
using FluentValidation;

namespace Taskist.Web.Helpers.Validators.Masters;

public class ClientValidator : AbstractValidator<ClientModel>
{
    public ClientValidator(ILocalizationService localizationService,
        IClientService clientService)
    {
        RuleFor(r => r.Name)
            .NotEmpty().WithMessage(localizationService.GetResource("ClientModel.Name.RequiredMsg"))
            .MaximumLength(100).WithMessage(localizationService.GetResource("ClientModel.Name.MaxLengthMsg"))
            .MustAwait(async (x, context) =>
            {
                if (x.Id > 0)
                {
                    var editedEntity = await clientService.GetByNameAsync(x.Name);
                    return editedEntity == null || editedEntity.Id == x.Id;
                }
                var entity = await clientService.GetByNameAsync(x.Name);
                return entity == null;
            }).WithMessage(localizationService.GetResource("ClientModel.Name.UniqueMsg"));

        RuleFor(r => r.Description)
            .MaximumLength(250).WithMessage(localizationService.GetResource("ClientModel.Description.MaxLengthMsg"));

        RuleFor(r => r.ContactPerson)
            .MaximumLength(100).WithMessage(localizationService.GetResource("ClientModel.ContactPerson.MaxLengthMsg"));

        RuleFor(r => r.PhoneNumber)
            .MaximumLength(100).WithMessage(localizationService.GetResource("ClientModel.PhoneNumber.MaxLengthMsg"));

        RuleFor(r => r.Email)
            .EmailAddress().WithMessage(localizationService.GetResource("ClientModel.Email.InvalidEmailMsg"))
            .MaximumLength(250).WithMessage(localizationService.GetResource("ClientModel.Email.MaxLengthMsg"));

        RuleFor(r => r.Website)
            .MaximumLength(750).WithMessage(localizationService.GetResource("ClientModel.Website.MaxLengthMsg"));
    }
}