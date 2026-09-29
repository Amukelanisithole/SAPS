using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.EntityConfiguration
{
    public class DocketDiaryEntryConfiguration : IEntityTypeConfiguration<DocketDiaryEntry>
    {
        public void Configure(EntityTypeBuilder<DocketDiaryEntry> modelbuilder)
        {
            modelbuilder.HasKey(e => e.Id);

            modelbuilder.Property(e => e.Id).ValueGeneratedOnAdd();

            modelbuilder.Property(e => e.EntryTypeId).IsRequired();
            modelbuilder.Property(e => e.ActionDate).IsRequired().HasColumnType("datetime2(0)");
            modelbuilder.Property(e => e.Notes).HasMaxLength(2000).IsRequired();
            modelbuilder.Property(e => e.SharedWithComplainant).IsRequired();
            modelbuilder.Property(e => e.CreatedDate).IsRequired().HasColumnType("datetime2(0)");

            modelbuilder.HasIndex(e => e.DocketId);

            modelbuilder
                .HasOne(e => e.NavDocket)
                .WithMany(d => d.NavDiaryEntries)
                .HasForeignKey(e => e.DocketId)
                .OnDelete(DeleteBehavior.NoAction);

            modelbuilder
                .HasOne(e => e.NavAuthor)
                .WithMany()
                .HasForeignKey(e => e.AuthorId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
