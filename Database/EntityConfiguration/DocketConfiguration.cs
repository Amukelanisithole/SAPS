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

            // A preliminary report can be converted at most once, so the docket
            // holding the FK is the one-to-one dependent side.
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
        }
    }
}
