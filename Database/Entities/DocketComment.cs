namespace Database.Entities
{
    public class DocketComment
    {
        public long Id { get; set; }
        public long DocketId { get; set; }
        public long AuthorId { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }

        public Docket? NavDocket { get; set; }
        public User? NavAuthor { get; set; }
    }
}
