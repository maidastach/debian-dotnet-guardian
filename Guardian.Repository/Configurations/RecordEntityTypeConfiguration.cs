using Guardian.Domain.Entities.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Guardian.Repository.Configurations
{
    internal sealed class RecordEntityTypeConfiguration : IEntityTypeConfiguration<Record>
    {
        public void Configure(EntityTypeBuilder<Record> builder)
        {
            builder.ToTable("tb_Records");
            builder.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            builder.Property(e => e.FullPath).IsRequired().HasMaxLength(500);
            builder.Property(e => e.CreatedAt).IsRequired().HasColumnType("DateTimeOffset");
            builder.Property(e => e.ModifiedAt).IsRequired().HasColumnType("DateTimeOffset");
            builder.Property(e => e.IsUploaded).IsRequired();
            builder.Property(e => e.GoogleDriveId).HasMaxLength(255);
            builder.Property(e => e.UploadedAt).HasColumnType("DateTimeOffset");
            builder.Property(e => e.IsDeleted).IsRequired();
            builder.HasMany(e => e.RecordLogs).WithOne(r => r.Record).HasForeignKey(r => r.RecordId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}