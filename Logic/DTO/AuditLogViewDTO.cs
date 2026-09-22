namespace Logic.DTO
{
    public class AuditLogViewDTO
    {
        public long Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public long UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public long? DocketId { get; set; }
        public long? AffidavitId { get; set; }
        public string? AffidavitReferenceNumber { get; set; }
        public long? EmergencyAlertId { get; set; }
        public string? IpAddress { get; set; }
    }
}
