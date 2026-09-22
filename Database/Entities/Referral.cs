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

        public Docket? NavDocket { get; set; }
        public Station? NavFromStation { get; set; }
        public Station? NavToStation { get; set; }
        public User? NavReferredByOfficer { get; set; }
    }
}
