using Taskist.Core.Enums;
using Taskist.Service.Git;
using Taskist.Service.Localization;
using Taskist.Web.Models.Masters;
using FluentValidation;

namespace Taskist.Web.Helpers.Validators.Masters;

public class GitConnectionValidator : AbstractValidator<GitConnectionModel>
{
    public GitConnectionValidator(ILocalizationService localizationService)
    {
        RuleFor(r => r.Name)
            .NotEmpty().WithMessage(localizationService.GetResource("GitConnectionModel.Name.RequiredMsg"))
            .MaximumLength(100);

        RuleFor(r => r.ProviderType)
            .Must(v => Enum.IsDefined(typeof(GitProviderType), v));

        RuleFor(r => r.AuthMode)
            .Must(v => Enum.IsDefined(typeof(GitAuthMode), v))
            .Must((model, v) => v != (int)GitAuthMode.GitHubApp || model.ProviderType == (int)GitProviderType.GitHub)
            .WithMessage(localizationService.GetResource("GitConnectionModel.AuthMode.GitHubOnlyMsg"));

        RuleFor(r => r.BaseUrl)
            .NotEmpty().WithMessage(localizationService.GetResource("GitConnectionModel.BaseUrl.RequiredMsg"))
            .MaximumLength(500)
            .Must((model, url) => GitHttp.ValidateBaseUrl(url, model.AllowInsecureHttp) == null)
            .WithMessage((model, url) => GitHttp.ValidateBaseUrl(url, model.AllowInsecureHttp) ?? string.Empty);

        RuleFor(r => r.ApiUrl)
            .MaximumLength(500)
            .Must((model, url) => string.IsNullOrWhiteSpace(url) || GitHttp.ValidateBaseUrl(url, model.AllowInsecureHttp) == null)
            .WithMessage((model, url) => GitHttp.ValidateBaseUrl(url, model.AllowInsecureHttp) ?? string.Empty);

        RuleFor(r => r.AppId)
            .MaximumLength(50)
            .Matches(@"^\d*$");

        RuleFor(r => r.ServiceToken).MaximumLength(1000);
        RuleFor(r => r.AppPrivateKey).MaximumLength(10000);
        RuleFor(r => r.OAuthClientId).MaximumLength(200);
        RuleFor(r => r.OAuthClientSecret).MaximumLength(500);
    }
}
