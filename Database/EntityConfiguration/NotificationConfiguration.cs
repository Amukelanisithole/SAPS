using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.EntityConfiguration
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> modelbuilder)
        {
            modelbuilder.HasKey(n => n.Id);

            modelbuilder.Property(n => n.Id).ValueGeneratedOnAdd();

            modelbuilder.Property(n => n.Message).IsRequired();
            modelbuilder.Property(n => n.SentDate).IsRequired().HasColumnType("datetime2(0)");
            modelbuilder.Property(n => n.ReadStatus).IsRequired();

            modelbuilder
                .HasOne(n => n.NavUser)
                .WithMany(u => u.NavNotifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelbuilder
                .HasOne(n => n.NavDocket)
                .WithMany(d => d.NavNotifications)
                .HasForeignKey(n => n.DocketId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
