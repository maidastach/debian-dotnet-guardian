using Guardian.Repository.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Guardian.Repository.Extensions
{
    public static class MigrationExtensions
    {
        public static void ApplyMigrations(this IServiceCollection services)
        {
            var scopeFactory = services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MonitoringContext>();

            dbContext.Database.Migrate();
        }
    }
}