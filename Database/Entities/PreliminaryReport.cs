namespace Database.Entities
{
    public class PreliminaryReport
    {
        public long Id { get; set; }
        public long ComplainantId { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public string IncidentType { get; set; } = string.Empty;
        public string IncidentDescription { get; set; } = string.Empty;
        public DateTime IncidentDate { get; set; }
        public string IncidentLocation { get; set; } = string.Empty;
        public string? SupportingInfo { get; set; }
        public DateTime SubmissionTimestamp { get; set; }
        public long StatusId { get; set; }
        public double? ComplainantLatitude { get; set; }
        public double? ComplainantLongitude { get; set; }
        public long? NearestStationId { get; set; }
        public long? ReviewedByOfficerId { get; set; }
        public string? RejectionReason { get; set; }

        public User? NavComplainant { get; set; }
        public User? NavReviewedByOfficer { get; set; }
        public Docket? NavDocket { get; set; }
        public Station? NavNearestStation { get; set; }
    }
}
