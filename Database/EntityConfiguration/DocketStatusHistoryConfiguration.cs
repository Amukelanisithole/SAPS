using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.EntityConfiguration
{
    public class DocketStatusHistoryConfiguration : IEntityTypeConfiguration<DocketStatusHistory>
    {
        public void Configure(EntityTypeBuilder<DocketStatusHistory> modelbuilder)
        {
            modelbuilder.HasKey(h => h.Id);

            modelbuilder.Property(h => h.Id).ValueGeneratedOnAdd();

            modelbuilder.Property(h => h.FromStatusId).IsRequired(false);
            modelbuilder.Property(h => h.ToStatusId).IsRequired();
            modelbuilder.Property(h => h.Reason).HasMaxLength(1000).IsRequired(false);
            modelbuilder.Property(h => h.ChangedDate).IsRequired().HasColumnType("datetime2(0)");

            modelbuilder.HasIndex(h => h.DocketId);

            modelbuilder
                .HasOne(h => h.NavDocket)
                .WithMany(d => d.NavStatusHistory)
                .HasForeignKey(h => h.DocketId)
                .OnDelete(DeleteBehavior.NoAction);

            modelbuilder
                .HasOne(h => h.NavChangedBy)
                .WithMany()
                .HasForeignKey(h => h.ChangedByUserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
