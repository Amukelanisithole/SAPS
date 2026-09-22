using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.EntityConfiguration
{
    public class DocketCommentConfiguration : IEntityTypeConfiguration<DocketComment>
    {
        public void Configure(EntityTypeBuilder<DocketComment> modelbuilder)
        {
            modelbuilder.HasKey(c => c.Id);

            modelbuilder.Property(c => c.Id).ValueGeneratedOnAdd();

            modelbuilder.Property(c => c.Message).HasMaxLength(2000).IsRequired();
            modelbuilder.Property(c => c.CreatedDate).IsRequired().HasColumnType("datetime2(0)");

            modelbuilder.HasIndex(c => c.DocketId);

            modelbuilder
                .HasOne(c => c.NavDocket)
                .WithMany(d => d.NavComments)
                .HasForeignKey(c => c.DocketId)
                .OnDelete(DeleteBehavior.NoAction);

            modelbuilder
                .HasOne(c => c.NavAuthor)
                .WithMany()
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
