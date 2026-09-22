namespace Database.Entities
{
    public class Notification
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long? DocketId { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime SentDate { get; set; }
        public bool ReadStatus { get; set; }

        public User? NavUser { get; set; }
        public Docket? NavDocket { get; set; }
    }
}
