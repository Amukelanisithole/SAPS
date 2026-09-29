namespace Database.Entities
{
    public class Station
    {
        public long Id { get; set; }
        public string StationName { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public long? OsmNodeId { get; set; }

        public ICollection<User> NavUsers { get; set; } = new List<User>();
        public ICollection<Docket> NavDockets { get; set; } = new List<Docket>();
        public ICollection<Referral> NavReferralsFrom { get; set; } = new List<Referral>();
        public ICollection<Referral> NavReferralsTo { get; set; } = new List<Referral>();
        public ICollection<PreliminaryReport> NavNearestReports { get; set; } = new List<PreliminaryReport>();
        public ICollection<Affidavit> NavAffidavits { get; set; } = new List<Affidavit>();
        public ICollection<EmergencyAlert> NavEmergencyAlerts { get; set; } = new List<EmergencyAlert>();
    }
}
