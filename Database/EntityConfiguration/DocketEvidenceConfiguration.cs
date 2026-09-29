using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.EntityConfiguration
{
    public class DocketEvidenceConfiguration : IEntityTypeConfiguration<DocketEvidence>
    {
        public void Configure(EntityTypeBuilder<DocketEvidence> modelbuilder)
        {
            modelbuilder.HasKey(e => e.Id);

            modelbuilder.Property(e => e.Id).ValueGeneratedOnAdd();

            modelbuilder.Property(e => e.Description).HasMaxLength(500).IsRequired();
            modelbuilder.Property(e => e.FilePath).HasMaxLength(400).IsRequired();
            modelbuilder.Property(e => e.OriginalFileName).HasMaxLength(260).IsRequired();
            modelbuilder.Property(e => e.ContentType).HasMaxLength(100).IsRequired();
            modelbuilder.Property(e => e.FileSizeBytes).IsRequired();
            modelbuilder.Property(e => e.UploadedDate).IsRequired().HasColumnType("datetime2(0)");
            modelbuilder.Property(e => e.SubmittedByComplainant).IsRequired();
            modelbuilder.Property(e => e.ReviewStatusId).IsRequired();
            modelbuilder.Property(e => e.ReviewedDate).IsRequired(false).HasColumnType("datetime2(0)");
            modelbuilder.Property(e => e.ReviewNote).HasMaxLength(500).IsRequired(false);

            modelbuilder.HasIndex(e => e.DocketId);

            modelbuilder
                .HasOne(e => e.NavDocket)
                .WithMany(d => d.NavEvidence)
                .HasForeignKey(e => e.DocketId)
                .OnDelete(DeleteBehavior.NoAction);

            modelbuilder
                .HasOne(e => e.NavUploadedBy)
                .WithMany()
                .HasForeignKey(e => e.UploadedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelbuilder
                .HasOne(e => e.NavReviewedBy)
                .WithMany()
                .HasForeignKey(e => e.ReviewedByUserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
