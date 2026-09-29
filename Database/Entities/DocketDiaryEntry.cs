namespace Database.Entities
{
    public class DocketDiaryEntry
    {
        public long Id { get; set; }
        public long DocketId { get; set; }
        public long AuthorId { get; set; }
        public long EntryTypeId { get; set; }
        public DateTime ActionDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public bool SharedWithComplainant { get; set; }
        public DateTime CreatedDate { get; set; }

        public Docket? NavDocket { get; set; }
        public User? NavAuthor { get; set; }
    }
}
