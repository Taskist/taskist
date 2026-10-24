using FluentValidation;
using Taskist.Service.Localization;
using Taskist.Web.Models.Masters;

namespace Taskist.Web.Helpers.Validators.Users;

public class AddressValidator : AbstractValidator<AddressModel>
{
    public AddressValidator(ILocalizationService localizationService)
    {
        RuleFor(r => r.Address1)
            .NotEmpty().WithMessage(localizationService.GetResource("AddressModel.Address1.RequiredMsg"))
            .MaximumLength(750).WithMessage(localizationService.GetResource("AddressModel.Address1.MaxLengthMsg"));

        RuleFor(r => r.City)
            .NotEmpty().WithMessage(localizationService.GetResource("AddressModel.City.RequiredMsg"))
            .MaximumLength(100).WithMessage(localizationService.GetResource("AddressModel.City.MaxLengthMsg"));

        RuleFor(r => r.ZipPostalCode)
            .NotEmpty().WithMessage(localizationService.GetResource("AddressModel.ZipPostalCode.RequiredMsg"))
            .MaximumLength(20).WithMessage(localizationService.GetResource("AddressModel.ZipPostalCode.MaxLengthMsg"));
    }
}