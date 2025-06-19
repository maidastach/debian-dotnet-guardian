using Guardian.Domain.Entities.Records;
using Guardian.Repository.Interfaces;
using Guardian.Repository.Migrations;
using Guardian.Repository.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Guardian.Repository.DependencyInjection
{
    public static class RepositoryDependencies
    {
        public static IServiceCollection AddRepositoryDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IRepository<Record>, Repository<Record>>();
            services.AddScoped<IRepository<RecordLog>, Repository<RecordLog>>();

            services.AddDbContext<MonitoringContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
            );

            return services;
        }
    }
}