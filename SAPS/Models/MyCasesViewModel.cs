using Database.Entities;
using Logic.Model;

namespace SAPS.Models
{
    public class MyCasesViewModel
    {
        public PagedResult<AssignedDocketItem> Dockets { get; set; } = new();
        public IEnumerable<PreliminaryReport> ReportsUnderReview { get; set; } = Enumerable.Empty<PreliminaryReport>();
        public bool IncludeFinished { get; set; }
        public int OverdueThresholdDays { get; set; }
    }
}
