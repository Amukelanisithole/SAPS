using System.Text.Json.Serialization;

namespace Database.Entities
{
    public class User
    {
        public long Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string IdNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        // Never serialized: this is a bcrypt hash and has no reason to leave the server.
        [JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;
        public long RoleId { get; set; }
        public long? StationId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLogin { get; set; }
        public int FailedLoginAttempts { get; set; }
        public DateTime? LockoutEnd { get; set; }
        // Defaults to true at the DB level so admin-provisioned staff
        // accounts and the seeded System Administrator are never blocked —
        // only self-registration (AuthService.RegisterAsync) explicitly
        // sets this false and starts the verify-by-email flow.
        public bool EmailVerified { get; set; } = true;

        public Role? NavRole { get; set; }
        public Station? NavStation { get; set; }
        public ICollection<PreliminaryReport> NavPreliminaryReports { get; set; } = new List<PreliminaryReport>();
        public ICollection<Affidavit> NavAffidavits { get; set; } = new List<Affidavit>();
        public ICollection<Affidavit> NavAffidavitsStamped { get; set; } = new List<Affidavit>();
        public ICollection<EmergencyAlert> NavEmergencyAlerts { get; set; } = new List<EmergencyAlert>();
        public ICollection<EmergencyAlert> NavEmergencyAlertsAcknowledged { get; set; } = new List<EmergencyAlert>();
        public ICollection<Docket> NavDocketsReviewed { get; set; } = new List<Docket>();
        public ICollection<Referral> NavReferralsMade { get; set; } = new List<Referral>();
        public ICollection<Escalation> NavEscalationsLodged { get; set; } = new List<Escalation>();
        public ICollection<AuditLog> NavAuditLogs { get; set; } = new List<AuditLog>();
        public ICollection<Notification> NavNotifications { get; set; } = new List<Notification>();
        public ICollection<BackupRecord> NavBackupRecordsPerformed { get; set; } = new List<BackupRecord>();
    }
}
