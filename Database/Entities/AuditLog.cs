namespace Database.Entities
{
    public class AuditLog
    {
        public long Id { get; set; }
        public long? DocketId { get; set; }
        public long? AffidavitId { get; set; }
        public long? EmergencyAlertId { get; set; }
        public long UserId { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? IpAddress { get; set; }

        public Docket? NavDocket { get; set; }
        public Affidavit? NavAffidavit { get; set; }
        public EmergencyAlert? NavEmergencyAlert { get; set; }
        public User? NavUser { get; set; }
    }
}
