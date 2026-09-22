namespace Database.Entities
{
    public class Docket
    {
        public long Id { get; set; }
        public long ReportId { get; set; }
        public string? CasNumber { get; set; }
        public long StationId { get; set; }
        public long ReviewingOfficerId { get; set; }
        public long StatusId { get; set; }
        public DateTime OpenedDate { get; set; }
        public DateTime LastUpdated { get; set; }

        public PreliminaryReport? NavReport { get; set; }
        public Station? NavStation { get; set; }
        public User? NavReviewingOfficer { get; set; }
        public ICollection<Referral> NavReferrals { get; set; } = new List<Referral>();
        public ICollection<Escalation> NavEscalations { get; set; } = new List<Escalation>();
        public ICollection<AuditLog> NavAuditLogs { get; set; } = new List<AuditLog>();
        public ICollection<Notification> NavNotifications { get; set; } = new List<Notification>();
        public ICollection<DocketComment> NavComments { get; set; } = new List<DocketComment>();
    }
}
