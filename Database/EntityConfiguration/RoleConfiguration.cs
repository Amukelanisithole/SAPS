using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.EntityConfiguration
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> modelbuilder)
        {
            modelbuilder.HasKey(r => r.Id);

            modelbuilder.Property(r => r.Id).ValueGeneratedNever();

            modelbuilder.Property(r => r.Name).HasMaxLength(50).IsRequired();

            modelbuilder.HasIndex(r => r.Name).IsUnique();
        }
    }
}
