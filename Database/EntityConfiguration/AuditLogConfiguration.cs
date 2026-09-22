using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.EntityConfiguration
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> modelbuilder)
        {
            modelbuilder.HasKey(a => a.Id);

            modelbuilder.Property(a => a.Id).ValueGeneratedOnAdd();

            modelbuilder.Property(a => a.ActionType).HasMaxLength(100).IsRequired();
            modelbuilder.Property(a => a.Timestamp).IsRequired().HasColumnType("datetime2(0)");
            modelbuilder.Property(a => a.IpAddress).HasMaxLength(45).IsRequired(false);

            modelbuilder
                .HasOne(a => a.NavDocket)
                .WithMany(d => d.NavAuditLogs)
                .HasForeignKey(a => a.DocketId)
                .OnDelete(DeleteBehavior.NoAction);

            modelbuilder
                .HasOne(a => a.NavAffidavit)
                .WithMany(af => af.NavAuditLogs)
                .HasForeignKey(a => a.AffidavitId)
                .OnDelete(DeleteBehavior.NoAction);

            modelbuilder
                .HasOne(a => a.NavEmergencyAlert)
                .WithMany(e => e.NavAuditLogs)
                .HasForeignKey(a => a.EmergencyAlertId)
                .OnDelete(DeleteBehavior.NoAction);

            modelbuilder
                .HasOne(a => a.NavUser)
                .WithMany(u => u.NavAuditLogs)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
