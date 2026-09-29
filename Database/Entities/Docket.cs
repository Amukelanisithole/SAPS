namespace Database.Entities
{
    public class Docket
    {
        public long Id { get; set; }
        public long ReportId { get; set; }
        public string? CasNumber { get; set; }
        public long StationId { get; set; }
        public long ReviewingOfficerId { get; set; }
        public long? InvestigatingOfficerId { get; set; }
        public long StatusId { get; set; }
        public DateTime OpenedDate { get; set; }
        public DateTime LastUpdated { get; set; }

        public long? ClosureOutcomeId { get; set; }
        public string? ClosureNotes { get; set; }
        public DateTime? ClosedDate { get; set; }
        public long? ClosedByUserId { get; set; }

        public DateTime? LastOverdueAlertDate { get; set; }

        public PreliminaryReport? NavReport { get; set; }
        public Station? NavStation { get; set; }
        public User? NavReviewingOfficer { get; set; }
        public User? NavInvestigatingOfficer { get; set; }
        public User? NavClosedBy { get; set; }
        public ICollection<Referral> NavReferrals { get; set; } = new List<Referral>();
        public ICollection<Escalation> NavEscalations { get; set; } = new List<Escalation>();
        public ICollection<AuditLog> NavAuditLogs { get; set; } = new List<AuditLog>();
        public ICollection<Notification> NavNotifications { get; set; } = new List<Notification>();
        public ICollection<DocketComment> NavComments { get; set; } = new List<DocketComment>();
        public ICollection<DocketEvidence> NavEvidence { get; set; } = new List<DocketEvidence>();
        public ICollection<DocketStatusHistory> NavStatusHistory { get; set; } = new List<DocketStatusHistory>();
        public ICollection<DocketDiaryEntry> NavDiaryEntries { get; set; } = new List<DocketDiaryEntry>();
    }
}
