namespace Database.Entities
{
    public class Escalation
    {
        public long Id { get; set; }
        public long DocketId { get; set; }
        public long ComplainantId { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime EscalationDate { get; set; }
        public long StatusId { get; set; }

        public Docket? NavDocket { get; set; }
        public User? NavComplainant { get; set; }
    }
}
