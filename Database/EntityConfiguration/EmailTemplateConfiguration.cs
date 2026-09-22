using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.EntityConfiguration
{
    public class EmailTemplateConfiguration : IEntityTypeConfiguration<EmailTemplate>
    {
        public void Configure(EntityTypeBuilder<EmailTemplate> modelbuilder)
        {
            modelbuilder.HasKey(t => t.Id);

            modelbuilder.Property(t => t.Id).ValueGeneratedOnAdd();

            modelbuilder.Property(t => t.Name).HasMaxLength(128).IsRequired();
            modelbuilder.Property(t => t.Description).HasMaxLength(512);
            modelbuilder.Property(t => t.Subject).HasMaxLength(256).IsRequired();
            modelbuilder.Property(t => t.Body).IsRequired();
            modelbuilder.Property(t => t.Placeholders).HasMaxLength(512);
            modelbuilder.Property(t => t.IsActive).IsRequired().HasDefaultValue(true);

            modelbuilder.HasIndex(t => t.Name).IsUnique();
        }
    }
}
