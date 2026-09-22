using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.EntityConfiguration
{
    public class AffidavitConfiguration : IEntityTypeConfiguration<Affidavit>
    {
        public void Configure(EntityTypeBuilder<Affidavit> modelbuilder)
        {
            modelbuilder.HasKey(a => a.Id);

            modelbuilder.Property(a => a.Id).ValueGeneratedOnAdd();

            modelbuilder.Property(a => a.ReferenceNumber).HasMaxLength(30).IsRequired();
            modelbuilder.Property(a => a.FullName).HasMaxLength(200).IsRequired();
            modelbuilder.Property(a => a.IdNumber).HasMaxLength(20).IsRequired();
            modelbuilder.Property(a => a.WorkingAddress).HasMaxLength(500).IsRequired();
            modelbuilder.Property(a => a.TelWork).HasMaxLength(30).IsRequired(false);
            modelbuilder.Property(a => a.TelHome).HasMaxLength(30).IsRequired(false);
            modelbuilder.Property(a => a.TelCell).HasMaxLength(30).IsRequired();
            modelbuilder.Property(a => a.DeclarationText).IsRequired();
            modelbuilder.Property(a => a.VerificationPhotoPath).HasMaxLength(400).IsRequired();
            modelbuilder.Property(a => a.IdDocumentPath).HasMaxLength(400).IsRequired(false);
            modelbuilder.Property(a => a.IdDocumentContentType).HasMaxLength(100).IsRequired(false);
            modelbuilder.Property(a => a.SubmissionTimestamp).IsRequired().HasColumnType("datetime2(0)");
            modelbuilder.Property(a => a.StatusId).IsRequired();
            modelbuilder.Property(a => a.ComplainantLatitude).IsRequired(false);
            modelbuilder.Property(a => a.ComplainantLongitude).IsRequired(false);

            modelbuilder.Property(a => a.StampedByOfficerName).HasMaxLength(200).IsRequired(false);
            modelbuilder.Property(a => a.StampedByOfficerForceNumber).HasMaxLength(50).IsRequired(false);
            modelbuilder.Property(a => a.StampedByOfficerRank).HasMaxLength(100).IsRequired(false);
            modelbuilder.Property(a => a.StampedDate).IsRequired(false).HasColumnType("datetime2(0)");
            modelbuilder.Property(a => a.StampedPdfPath).HasMaxLength(400).IsRequired(false);
            modelbuilder.Property(a => a.RejectionReason).HasMaxLength(1000).IsRequired(false);

            modelbuilder.HasIndex(a => a.ReferenceNumber).IsUnique();

            modelbuilder
                .HasOne(a => a.NavComplainant)
                .WithMany(u => u.NavAffidavits)
                .HasForeignKey(a => a.ComplainantId)
                .OnDelete(DeleteBehavior.NoAction);

            modelbuilder
                .HasOne(a => a.NavNearestStation)
                .WithMany(s => s.NavAffidavits)
                .HasForeignKey(a => a.NearestStationId)
                .OnDelete(DeleteBehavior.NoAction);

            modelbuilder
                .HasOne(a => a.NavStampedByOfficer)
                .WithMany(u => u.NavAffidavitsStamped)
                .HasForeignKey(a => a.StampedByOfficerId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
