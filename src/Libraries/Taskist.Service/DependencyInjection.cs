using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using Taskist.Core.Common;

namespace Taskist.Service;

public static class DependencyInjection
{
	public static IServiceCollection AddApplicationServices(this IServiceCollection services)
	{
		services.AddCommonServices();

		services.Scan(scan => scan
			.FromAssemblies(typeof(DependencyInjection).Assembly)
			.AddClasses(c => c.InNamespaces(
				"Taskist.Service.Announcements",
				"Taskist.Service.Authentication",
				"Taskist.Service.Users",
				"Taskist.Service.External",
				"Taskist.Service.Git",
				"Taskist.Service.Localization",
				"Taskist.Service.Logging",
				"Taskist.Service.Masters",
				"Taskist.Service.Messages",
				"Taskist.Service.Reports",
				"Taskist.Service.Security",
				"Taskist.Service.Wiki",
				"Taskist.Service.WorkItems"))
			.UsingRegistrationStrategy(RegistrationStrategy.Skip)
			.AsMatchingInterface()
			.WithScopedLifetime());

		services.AddHttpClient(Git.GitHttp.ClientName, client => client.Timeout = Git.GitHttp.Timeout)
			.ConfigurePrimaryHttpMessageHandler(() => Git.GitHttp.CreateHandler(allowLocal: false));
		services.AddHttpClient(Messages.NotificationChannelService.ClientName, client => client.Timeout = TimeSpan.FromSeconds(15))
			.ConfigurePrimaryHttpMessageHandler(() => Git.GitHttp.CreateHandler(allowLocal: false));
		services.AddHttpClient(Git.GitHttp.LocalClientName, client => client.Timeout = Git.GitHttp.Timeout)
			.ConfigurePrimaryHttpMessageHandler(() => Git.GitHttp.CreateHandler(allowLocal: true));

		return services;
	}
}
