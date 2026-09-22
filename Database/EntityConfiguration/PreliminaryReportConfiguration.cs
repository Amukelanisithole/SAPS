using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.EntityConfiguration
{
    public class PreliminaryReportConfiguration : IEntityTypeConfiguration<PreliminaryReport>
    {
        public void Configure(EntityTypeBuilder<PreliminaryReport> modelbuilder)
        {
            modelbuilder.HasKey(r => r.Id);

            modelbuilder.Property(r => r.Id).ValueGeneratedOnAdd();

            modelbuilder.Property(r => r.ReferenceNumber).HasMaxLength(30).IsRequired();
            modelbuilder.Property(r => r.IncidentType).HasMaxLength(100).IsRequired();
            modelbuilder.Property(r => r.IncidentDescription).IsRequired();
            modelbuilder.Property(r => r.IncidentDate).IsRequired().HasColumnType("datetime2(0)");
            modelbuilder.Property(r => r.IncidentLocation).HasMaxLength(500).IsRequired();
            modelbuilder.Property(r => r.SupportingInfo).IsRequired(false);
            modelbuilder.Property(r => r.SubmissionTimestamp).IsRequired().HasColumnType("datetime2(0)");
            modelbuilder.Property(r => r.StatusId).IsRequired();
            modelbuilder.Property(r => r.ComplainantLatitude).IsRequired(false);
            modelbuilder.Property(r => r.ComplainantLongitude).IsRequired(false);

            modelbuilder.HasIndex(r => r.ReferenceNumber).IsUnique();

            modelbuilder
                .HasOne(r => r.NavComplainant)
                .WithMany(u => u.NavPreliminaryReports)
                .HasForeignKey(r => r.ComplainantId)
                .OnDelete(DeleteBehavior.NoAction);

            modelbuilder
                .HasOne(r => r.NavNearestStation)
                .WithMany(s => s.NavNearestReports)
                .HasForeignKey(r => r.NearestStationId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
