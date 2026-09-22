namespace Database.Entities
{
    // Digitizes the standard SAPS "Affidavit/Statement" form: a complainant
    // declares a statement under oath, the system routes it to their nearest
    // station (same geolocation flow as PreliminaryReport), and an officer
    // there stands in for the Commissioner of Oaths, certifying it and
    // producing the stamped PDF the deponent would otherwise walk out with.
    public class Affidavit
    {
        public long Id { get; set; }
        public long ComplainantId { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;

        // Deponent details are snapshotted at submission time, independent of
        // the account's own profile — this is a legal record and must not
        // silently change if the user later edits their profile.
        public string FullName { get; set; } = string.Empty;
        public string IdNumber { get; set; } = string.Empty;
        public int Age { get; set; }
        public string WorkingAddress { get; set; } = string.Empty;
        public string? TelWork { get; set; }
        public string? TelHome { get; set; }
        public string TelCell { get; set; } = string.Empty;

        public string DeclarationText { get; set; } = string.Empty;
        public bool ObjectionToOath { get; set; }

        // Relative path (under AppSettings.UploadsRootPath) to the live
        // camera capture taken at submission time to confirm identity.
        public string VerificationPhotoPath { get; set; } = string.Empty;

        // A photo/scan of the deponent's ID document (book/card), supplementing
        // the live verification photo above rather than replacing it — required
        // for every affidavit submitted from now on, but nullable here because
        // it was optional when this column was first introduced, so earlier
        // rows may not have one.
        public string? IdDocumentPath { get; set; }
        public string? IdDocumentContentType { get; set; }

        public DateTime SubmissionTimestamp { get; set; }
        public long StatusId { get; set; }

        public double? ComplainantLatitude { get; set; }
        public double? ComplainantLongitude { get; set; }
        public long? NearestStationId { get; set; }

        // Populated only once an officer at the routed station stamps
        // (or rejects) the affidavit.
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
