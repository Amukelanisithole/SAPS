using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.EntityConfiguration
{
    public class DocketConfiguration : IEntityTypeConfiguration<Docket>
    {
        public void Configure(EntityTypeBuilder<Docket> modelbuilder)
        {
            modelbuilder.HasKey(d => d.Id);

            modelbuilder.Property(d => d.Id).ValueGeneratedOnAdd();

            modelbuilder.Property(d => d.CasNumber).HasMaxLength(50).IsRequired(false);
            modelbuilder.Property(d => d.StatusId).IsRequired();
            modelbuilder.Property(d => d.OpenedDate).IsRequired().HasColumnType("datetime2(0)");
            modelbuilder.Property(d => d.LastUpdated).IsRequired().HasColumnType("datetime2(0)");
            modelbuilder.Property(d => d.InvestigatingOfficerId).IsRequired(false);
            modelbuilder.Property(d => d.ClosureOutcomeId).IsRequired(false);
            modelbuilder.Property(d => d.ClosureNotes).HasMaxLength(2000).IsRequired(false);
            modelbuilder.Property(d => d.ClosedDate).IsRequired(false).HasColumnType("datetime2(0)");
            modelbuilder.Property(d => d.ClosedByUserId).IsRequired(false);
            modelbuilder.Property(d => d.LastOverdueAlertDate).IsRequired(false).HasColumnType("datetime2(0)");

            modelbuilder.HasIndex(d => d.ReportId).IsUnique();

            modelbuilder
                .HasOne(d => d.NavReport)
                .WithOne(r => r.NavDocket)
                .HasForeignKey<Docket>(d => d.ReportId)
                .OnDelete(DeleteBehavior.NoAction);

            modelbuilder
                .HasOne(d => d.NavStation)
                .WithMany(s => s.NavDockets)
                .HasForeignKey(d => d.StationId)
                .OnDelete(DeleteBehavior.NoAction);

            modelbuilder
                .HasOne(d => d.NavReviewingOfficer)
                .WithMany(u => u.NavDocketsReviewed)
                .HasForeignKey(d => d.ReviewingOfficerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelbuilder
                .HasOne(d => d.NavInvestigatingOfficer)
                .WithMany(u => u.NavDocketsInvestigating)
                .HasForeignKey(d => d.InvestigatingOfficerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelbuilder
                .HasOne(d => d.NavClosedBy)
                .WithMany()
                .HasForeignKey(d => d.ClosedByUserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
