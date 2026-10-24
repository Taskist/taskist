using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace Taskist.Core.Common;

public static class CommonServiceCollectionExtensions
{
    public static IServiceCollection AddCommonServices(this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblyOf<IHttpHelper>()
            .AddClasses(c => c.InNamespaces("Taskist.Core.Common"))
            .UsingRegistrationStrategy(RegistrationStrategy.Skip)
            .AsMatchingInterface()
            .WithScopedLifetime());

        return services;
    }
}
