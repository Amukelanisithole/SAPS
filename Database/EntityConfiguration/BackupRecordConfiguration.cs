using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.EntityConfiguration
{
    public class BackupRecordConfiguration : IEntityTypeConfiguration<BackupRecord>
    {
        public void Configure(EntityTypeBuilder<BackupRecord> modelbuilder)
        {
            modelbuilder.HasKey(b => b.Id);

            modelbuilder.Property(b => b.Id).ValueGeneratedOnAdd();

            modelbuilder.Property(b => b.BackupDate).IsRequired().HasColumnType("datetime2(0)");
            modelbuilder.Property(b => b.StatusId).IsRequired();
            modelbuilder.Property(b => b.RestorationTested).IsRequired();

            modelbuilder
                .HasOne(b => b.NavPerformedByUser)
                .WithMany(u => u.NavBackupRecordsPerformed)
                .HasForeignKey(b => b.PerformedBy)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
