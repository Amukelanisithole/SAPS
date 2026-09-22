using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.EntityConfiguration
{
    public class EscalationConfiguration : IEntityTypeConfiguration<Escalation>
    {
        public void Configure(EntityTypeBuilder<Escalation> modelbuilder)
        {
            modelbuilder.HasKey(e => e.Id);

            modelbuilder.Property(e => e.Id).ValueGeneratedOnAdd();

            modelbuilder.Property(e => e.Description).IsRequired();
            modelbuilder.Property(e => e.EscalationDate).IsRequired().HasColumnType("datetime2(0)");
            modelbuilder.Property(e => e.StatusId).IsRequired();

            modelbuilder
                .HasOne(e => e.NavDocket)
                .WithMany(d => d.NavEscalations)
                .HasForeignKey(e => e.DocketId)
                .OnDelete(DeleteBehavior.NoAction);

            modelbuilder
                .HasOne(e => e.NavComplainant)
                .WithMany(u => u.NavEscalationsLodged)
                .HasForeignKey(e => e.ComplainantId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
