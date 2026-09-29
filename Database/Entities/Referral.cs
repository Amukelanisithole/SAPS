namespace Database.Entities
{
    public class Referral
    {
        public long Id { get; set; }
        public long DocketId { get; set; }
        public long FromStationId { get; set; }
        public long ToStationId { get; set; }
        public long ReferredByOfficerId { get; set; }
        public DateTime ReferralDate { get; set; }
        public string Reason { get; set; } = string.Empty;

        public long StatusId { get; set; }
        public long? PreviousDocketStatusId { get; set; }
        public long? RespondedByUserId { get; set; }
        public DateTime? RespondedDate { get; set; }
        public string? ResponseNote { get; set; }

        public Docket? NavDocket { get; set; }
        public Station? NavFromStation { get; set; }
        public Station? NavToStation { get; set; }
        public User? NavReferredByOfficer { get; set; }
        public User? NavRespondedBy { get; set; }
    }
}
