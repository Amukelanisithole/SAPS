using Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Database.AppDBContext
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; }
        public DbSet<Station> Stations { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<PreliminaryReport> PreliminaryReports { get; set; }
        public DbSet<Docket> Dockets { get; set; }
        public DbSet<Referral> Referrals { get; set; }
        public DbSet<Escalation> Escalations { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<BackupRecord> BackupRecords { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<DocketComment> DocketComments { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }
        public DbSet<Affidavit> Affidavits { get; set; }
        public DbSet<EmergencyAlert> EmergencyAlerts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
