using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.EntityConfiguration
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> modelbuilder)
        {
            modelbuilder.HasKey(u => u.Id);

            modelbuilder.Property(u => u.Id).ValueGeneratedOnAdd();

            modelbuilder.Property(u => u.FullName).HasMaxLength(255).IsRequired();
            modelbuilder.Property(u => u.IdNumber).HasMaxLength(20).IsRequired();
            modelbuilder.Property(u => u.Email).HasMaxLength(255).IsRequired();
            modelbuilder.Property(u => u.PhoneNumber).HasMaxLength(30).IsRequired();
            modelbuilder.Property(u => u.PasswordHash).IsRequired();
            modelbuilder.Property(u => u.RoleId).IsRequired();
            modelbuilder.Property(u => u.IsActive).IsRequired().HasDefaultValue(true);
            modelbuilder.Property(u => u.CreatedDate).IsRequired().HasColumnType("datetime2(0)");
            modelbuilder.Property(u => u.LastLogin).HasColumnType("datetime2(0)");
            modelbuilder.Property(u => u.FailedLoginAttempts).IsRequired().HasDefaultValue(0);
            modelbuilder.Property(u => u.LockoutEnd).HasColumnType("datetime2(0)");
            modelbuilder.Property(u => u.EmailVerified).IsRequired().HasDefaultValue(true);

            modelbuilder.HasIndex(u => u.Email).IsUnique();
            modelbuilder.HasIndex(u => u.IdNumber).IsUnique();

            modelbuilder
                .HasOne(u => u.NavRole)
                .WithMany(r => r.NavUsers)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.NoAction);

            modelbuilder
                .HasOne(u => u.NavStation)
                .WithMany(s => s.NavUsers)
                .HasForeignKey(u => u.StationId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
