namespace Database.Entities
{
    public class Affidavit
    {
        public long Id { get; set; }
        public long ComplainantId { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;
        public string IdNumber { get; set; } = string.Empty;
        public int Age { get; set; }
        public string WorkingAddress { get; set; } = string.Empty;
        public string? TelWork { get; set; }
        public string? TelHome { get; set; }
        public string TelCell { get; set; } = string.Empty;

        public string DeclarationText { get; set; } = string.Empty;
        public bool ObjectionToOath { get; set; }

        public string VerificationPhotoPath { get; set; } = string.Empty;

        public string? IdDocumentPath { get; set; }
        public string? IdDocumentContentType { get; set; }

        public DateTime SubmissionTimestamp { get; set; }
        public long StatusId { get; set; }

        public double? ComplainantLatitude { get; set; }
        public double? ComplainantLongitude { get; set; }
        public long? NearestStationId { get; set; }

        public long? StampedByOfficerId { get; set; }
        public string? StampedByOfficerName { get; set; }
        public string? StampedByOfficerForceNumber { get; set; }
        public string? StampedByOfficerRank { get; set; }
        public DateTime? StampedDate { get; set; }
        public string? StampedPdfPath { get; set; }
        public string? RejectionReason { get; set; }

        public User? NavComplainant { get; set; }
        public Station? NavNearestStation { get; set; }
        public User? NavStampedByOfficer { get; set; }
        public ICollection<AuditLog> NavAuditLogs { get; set; } = new List<AuditLog>();
    }
}
