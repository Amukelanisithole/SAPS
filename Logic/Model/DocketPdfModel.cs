using Database.Entities;
using Logic.DTO;

namespace Logic.Model
{
    public class DocketPdfModel
    {
        public Docket Docket { get; set; } = null!;
        public IEnumerable<DocketStatusHistory> StatusHistory { get; set; } = Enumerable.Empty<DocketStatusHistory>();
        public IEnumerable<DocketDiaryEntry> Diary { get; set; } = Enumerable.Empty<DocketDiaryEntry>();
        public IEnumerable<Referral> Referrals { get; set; } = Enumerable.Empty<Referral>();
        public IEnumerable<Escalation> Escalations { get; set; } = Enumerable.Empty<Escalation>();
        public IEnumerable<DocketCommentViewDTO> Comments { get; set; } = Enumerable.Empty<DocketCommentViewDTO>();
        public IList<DocketPdfEvidenceItem> Evidence { get; set; } = new List<DocketPdfEvidenceItem>();
        public string GeneratedByName { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
    }

    public class DocketPdfEvidenceItem
    {
        public DocketEvidence Evidence { get; set; } = null!;
        public byte[]? ImageBytes { get; set; }
    }
}
