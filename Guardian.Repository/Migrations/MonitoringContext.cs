using Guardian.Domain.Entities.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Guardian.Repository.Configurations;

namespace Guardian.Repository.Migrations
{
    public class MonitoringContext(DbContextOptions<MonitoringContext> options, IConfiguration configuration) : DbContext(options)
    {
        private readonly IConfiguration _configuration = configuration;

        public DbSet<Record> Record { get; set; }
        public DbSet<RecordLog> RecordLog { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            new RecordEntityTypeConfiguration().Configure(modelBuilder.Entity<Record>());
            new RecordLogEntityTypeConfiguration().Configure(modelBuilder.Entity<RecordLog>());
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
#if DEBUG
            optionsBuilder.EnableSensitiveDataLogging();
#endif

            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_configuration.GetConnectionString("DefaultConnection"));
            }
        }
    }
}