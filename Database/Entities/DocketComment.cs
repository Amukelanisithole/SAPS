namespace Database.Entities
{
    // A message exchanged between the complainant who filed the case and the
    // SAPS staff working it, attached to the Docket rather than the original
    // PreliminaryReport since the docket is the record that persists for the
    // life of the investigation.
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
