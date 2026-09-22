using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.EntityConfiguration
{
    public class EmailVerificationTokenConfiguration : IEntityTypeConfiguration<EmailVerificationToken>
    {
        public void Configure(EntityTypeBuilder<EmailVerificationToken> modelbuilder)
        {
            modelbuilder.HasKey(t => t.Id);

            modelbuilder.Property(t => t.Id).ValueGeneratedOnAdd();

            modelbuilder.Property(t => t.TokenHash).HasMaxLength(128).IsRequired();
            modelbuilder.Property(t => t.ExpiresAt).IsRequired().HasColumnType("datetime2(0)");
            modelbuilder.Property(t => t.Used).IsRequired().HasDefaultValue(false);
            modelbuilder.Property(t => t.CreatedDate).IsRequired().HasColumnType("datetime2(0)");

            modelbuilder.HasIndex(t => t.TokenHash);
            modelbuilder.HasIndex(t => t.UserId);

            modelbuilder
                .HasOne(t => t.NavUser)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
