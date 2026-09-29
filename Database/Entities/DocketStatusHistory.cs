namespace Database.Entities
{
    public class DocketStatusHistory
    {
        public long Id { get; set; }
        public long DocketId { get; set; }
        public long? FromStatusId { get; set; }
        public long ToStatusId { get; set; }
        public long ChangedByUserId { get; set; }
        public string? Reason { get; set; }
        public DateTime ChangedDate { get; set; }

        public Docket? NavDocket { get; set; }
        public User? NavChangedBy { get; set; }
    }
}
