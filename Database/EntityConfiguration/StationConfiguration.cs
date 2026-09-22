using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.EntityConfiguration
{
    public class StationConfiguration : IEntityTypeConfiguration<Station>
    {
        public void Configure(EntityTypeBuilder<Station> modelbuilder)
        {
            modelbuilder.HasKey(s => s.Id);

            modelbuilder.Property(s => s.Id).ValueGeneratedOnAdd();

            modelbuilder.Property(s => s.StationName).HasMaxLength(255).IsRequired();
            modelbuilder.Property(s => s.Province).HasMaxLength(100).IsRequired();
            modelbuilder.Property(s => s.Address).HasMaxLength(500).IsRequired();
            modelbuilder.Property(s => s.ContactNumber).HasMaxLength(30).IsRequired();
            modelbuilder.Property(s => s.Latitude).IsRequired(false);
            modelbuilder.Property(s => s.Longitude).IsRequired(false);
            modelbuilder.Property(s => s.OsmNodeId).IsRequired(false);

            modelbuilder.HasIndex(s => s.OsmNodeId).IsUnique().HasFilter("[OsmNodeId] IS NOT NULL");
        }
    }
}
