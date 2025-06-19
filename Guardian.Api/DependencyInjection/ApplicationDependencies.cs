using System.Reflection;
using Guardian.Application.DependencyInjection;
using Guardian.Application.Services;
using Guardian.Domain.Configs;
using Guardian.Domain.DependencyInjection;
using Guardian.Infrastructure.DependencyInjection;

namespace Guardian.Api.DependencyInjection
{
    internal static class ApplicationDependencies
    {
        public static IServiceCollection AddApplicationDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            // Register configs
            services.Configure<GuardianConfig>(configuration.GetSection(nameof(GuardianConfig)));

            // Register Services
            services.AddHealthChecks();
            RegisterServices(services);
            services.AddHostedService<BackgroundTasksHandlerService>();

            services.AddMemoryCache();
            return services;
        }

        private static void RegisterServices(IServiceCollection services)
        {
            var assemblyTypes = ApplicationAssemblyReference.Assembly.GetTypes()
                .Concat(InfrastructureAssemblyReference.Assembly.GetTypes())
                .Where(t => t.GetCustomAttribute<ServiceLifetimeAttribute>() != null);

            foreach (var type in assemblyTypes)
            {
                var attribute = type.GetCustomAttribute<ServiceLifetimeAttribute>();
                foreach (var typeInterface in type.GetInterfaces())
                {
                    switch (attribute?.Lifetime)
                    {
                        case ServiceLifetime.Scoped:
                            services.AddScoped(typeInterface, type);
                            break;
                        case ServiceLifetime.Transient:
                            services.AddTransient(typeInterface, type);
                            break;
                        case ServiceLifetime.Singleton:
                            services.AddSingleton(typeInterface, type);
                            break;
                    }
                }
            }
        }
    }
}