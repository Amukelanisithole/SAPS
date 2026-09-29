using Database.Entities;

namespace Logic.Model
{
    public class AssignedDocketItem
    {
        public Docket Docket { get; set; } = null!;
        public DateTime LastActivity { get; set; }
    }
}
