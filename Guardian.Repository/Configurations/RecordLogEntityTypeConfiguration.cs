using Guardian.Domain.Entities.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Guardian.Repository.Configurations
{
    internal sealed class RecordLogEntityTypeConfiguration : IEntityTypeConfiguration<RecordLog>
    {
        public void Configure(EntityTypeBuilder<RecordLog> builder)
        {
            builder.ToTable("tb_RecordLogs");
            builder.Property(e => e.Date).IsRequired().HasColumnType("DateTimeOffset");
            builder.Property(e => e.Message).IsRequired();
            builder.Property(e => e.IsError).IsRequired();
            builder.Property(e => e.RecordId);
            builder.HasOne(e => e.Record).WithMany(r => r.RecordLogs).HasForeignKey(e => e.RecordId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}