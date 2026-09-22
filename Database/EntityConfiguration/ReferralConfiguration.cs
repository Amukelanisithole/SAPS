using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.EntityConfiguration
{
    public class ReferralConfiguration : IEntityTypeConfiguration<Referral>
    {
        public void Configure(EntityTypeBuilder<Referral> modelbuilder)
        {
            modelbuilder.HasKey(r => r.Id);

            modelbuilder.Property(r => r.Id).ValueGeneratedOnAdd();

            modelbuilder.Property(r => r.ReferralDate).IsRequired().HasColumnType("datetime2(0)");
            modelbuilder.Property(r => r.Reason).IsRequired();

            modelbuilder
                .HasOne(r => r.NavDocket)
                .WithMany(d => d.NavReferrals)
                .HasForeignKey(r => r.DocketId)
                .OnDelete(DeleteBehavior.NoAction);

            modelbuilder
                .HasOne(r => r.NavFromStation)
                .WithMany(s => s.NavReferralsFrom)
                .HasForeignKey(r => r.FromStationId)
                .OnDelete(DeleteBehavior.NoAction);

            modelbuilder
                .HasOne(r => r.NavToStation)
                .WithMany(s => s.NavReferralsTo)
                .HasForeignKey(r => r.ToStationId)
                .OnDelete(DeleteBehavior.NoAction);

            modelbuilder
                .HasOne(r => r.NavReferredByOfficer)
                .WithMany(u => u.NavReferralsMade)
                .HasForeignKey(r => r.ReferredByOfficerId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
