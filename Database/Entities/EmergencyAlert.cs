namespace Database.Entities
{
    // A fast, minimal-friction "flag my nearest station right now" action —
    // deliberately not routed through the full PreliminaryReport pipeline,
    // which asks for an incident type/description that someone in a genuine
    // emergency shouldn't have to stop and fill in. This is explicitly
    // supplementary to calling real emergency services, never a replacement —
    // see EmergencyController.Index, which leads with those numbers first.
    public class EmergencyAlert
    {
        public long Id { get; set; }
        public long ComplainantId { get; set; }
        public string? Message { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public long? NearestStationId { get; set; }

        public DateTime SubmissionTimestamp { get; set; }
        public long StatusId { get; set; }

        public long? AcknowledgedByOfficerId { get; set; }
        public DateTime? AcknowledgedDate { get; set; }

        public User? NavComplainant { get; set; }
        public Station? NavNearestStation { get; set; }
        public User? NavAcknowledgedByOfficer { get; set; }
        public ICollection<AuditLog> NavAuditLogs { get; set; } = new List<AuditLog>();
    }
}
