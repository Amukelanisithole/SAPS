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
        public IEnumerable<DocketStatusHistory> StatusHistory { get; set; } = Enumerable.Empty<DocketStatusHistory>();
        public IEnumerable<DocketEvidence> Evidence { get; set; } = Enumerable.Empty<DocketEvidence>();
        public IEnumerable<User> AssignableOfficers { get; set; } = Enumerable.Empty<User>();
        public IEnumerable<DocketDiaryEntry> Diary { get; set; } = Enumerable.Empty<DocketDiaryEntry>();
        public Referral? PendingReferral { get; set; }
        public int? DaysSinceActivity { get; set; }
        public int OverdueThresholdDays { get; set; }
        public IReadOnlyList<long> ManualTransitions { get; set; } = Array.Empty<long>();

        public bool IsStationStaff { get; set; }
        public bool IsCommander { get; set; }
        public bool IsReceivingStaff { get; set; }
        public bool CanRespondToReferral { get; set; }
        public bool CanCancelReferral { get; set; }
        public bool CanAddDiary { get; set; }
        public bool CanReviewEvidence { get; set; }
        public bool CanSubmitEvidence { get; set; }
        public bool CanUpdateStatus { get; set; }
        public bool CanRefer { get; set; }
        public bool CanEscalate { get; set; }
        public bool CanComment { get; set; }
        public bool CanEditCase { get; set; }
        public bool CanAddEvidence { get; set; }
        public bool CanRequestClosure { get; set; }
        public bool CanApproveClosure { get; set; }
        public bool CanDownload { get; set; }
        public long ViewingUserId { get; set; }

        public DocketStatusUpdateDTO StatusUpdate { get; set; } = new();
        public ReferralDTO Referral { get; set; } = new();
        public DocketCommentDTO NewComment { get; set; } = new();
        public DocketCasNumberDTO CasUpdate { get; set; } = new();
        public DocketAssignDTO Assign { get; set; } = new();
        public DocketClosureDTO Closure { get; set; } = new();
        public DocketEvidenceDTO NewEvidence { get; set; } = new();
        public DocketDiaryEntryDTO NewDiaryEntry { get; set; } = new();
    }
}
