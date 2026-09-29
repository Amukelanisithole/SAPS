namespace Database.Entities
{
    public class DocketEvidence
    {
        public long Id { get; set; }
        public long DocketId { get; set; }
        public long UploadedByUserId { get; set; }
        public string Description { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public DateTime UploadedDate { get; set; }

        public bool SubmittedByComplainant { get; set; }
        public long ReviewStatusId { get; set; }
        public long? ReviewedByUserId { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public string? ReviewNote { get; set; }

        public Docket? NavDocket { get; set; }
        public User? NavUploadedBy { get; set; }
        public User? NavReviewedBy { get; set; }
    }
}
