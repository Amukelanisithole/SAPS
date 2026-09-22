using Database.Entities;
using Logic.DTO;

namespace SAPS.Models
{
    public class DocketDetailsViewModel
    {
        public Docket Docket { get; set; } = null!;
        public IEnumerable<Referral> Referrals { get; set; } = Enumerable.Empty<Referral>();
        public IEnumerable<Escalation> Escalations { get; set; } = Enumerable.Empty<Escalation>();
        public IEnumerable<Station> AvailableStations { get; set; } = Enumerable.Empty<Station>();
        public IEnumerable<DocketCommentViewDTO> Comments { get; set; } = Enumerable.Empty<DocketCommentViewDTO>();
        public bool CanUpdateStatus { get; set; }
        public bool CanRefer { get; set; }
        public bool CanEscalate { get; set; }
        public bool CanComment { get; set; }
        public long ViewingUserId { get; set; }
        public DocketStatusUpdateDTO StatusUpdate { get; set; } = new();
        public ReferralDTO Referral { get; set; } = new();
        public DocketCommentDTO NewComment { get; set; } = new();
    }
}
