using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.EntityConfiguration
{
    public class EmergencyAlertConfiguration : IEntityTypeConfiguration<EmergencyAlert>
    {
        public void Configure(EntityTypeBuilder<EmergencyAlert> modelbuilder)
        {
            modelbuilder.HasKey(a => a.Id);

            modelbuilder.Property(a => a.Id).ValueGeneratedOnAdd();

            modelbuilder.Property(a => a.Message).HasMaxLength(500).IsRequired(false);
            modelbuilder.Property(a => a.SubmissionTimestamp).IsRequired().HasColumnType("datetime2(0)");
            modelbuilder.Property(a => a.StatusId).IsRequired();
            modelbuilder.Property(a => a.AcknowledgedDate).IsRequired(false).HasColumnType("datetime2(0)");

            modelbuilder
                .HasOne(a => a.NavComplainant)
                .WithMany(u => u.NavEmergencyAlerts)
                .HasForeignKey(a => a.ComplainantId)
                .OnDelete(DeleteBehavior.NoAction);

            modelbuilder
                .HasOne(a => a.NavNearestStation)
                .WithMany(s => s.NavEmergencyAlerts)
                .HasForeignKey(a => a.NearestStationId)
                .OnDelete(DeleteBehavior.NoAction);

            modelbuilder
                .HasOne(a => a.NavAcknowledgedByOfficer)
                .WithMany(u => u.NavEmergencyAlertsAcknowledged)
                .HasForeignKey(a => a.AcknowledgedByOfficerId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
