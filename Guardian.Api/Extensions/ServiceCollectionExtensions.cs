using Guardian.Api.DependencyInjection;
using Guardian.Repository.DependencyInjection;
using Guardian.Repository.Extensions;

namespace Guardian.Api.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        internal static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddApplicationDependencies(configuration)
                .AddRepositoryDependencies(configuration)
                .ApplyMigrations();
        }
    }
}