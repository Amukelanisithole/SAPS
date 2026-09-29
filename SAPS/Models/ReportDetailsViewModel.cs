using Database.Entities;
using Logic.DTO;

namespace SAPS.Models
{
    public class ReportDetailsViewModel
    {
        public PreliminaryReport Report { get; set; } = null!;
        public bool CanConvert { get; set; }
        public bool CanStartReview { get; set; }
        public bool CanReject { get; set; }
        public DocketConvertDTO Convert { get; set; } = new();
        public ReportRejectDTO Reject { get; set; } = new();
    }
}
