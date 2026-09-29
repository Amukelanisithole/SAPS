using Database.Entities;

namespace Logic.Model
{
    public class StationWorkQueue
    {
        public IReadOnlyList<Docket> PendingClosure { get; set; } = Array.Empty<Docket>();
        public IReadOnlyList<Docket> Unassigned { get; set; } = Array.Empty<Docket>();
        public IReadOnlyList<AssignedDocketItem> Overdue { get; set; } = Array.Empty<AssignedDocketItem>();
    }
}
