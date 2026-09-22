using Database.Entities;
using Logic.DTO;

namespace SAPS.Models
{
    public class AffidavitDetailsViewModel
    {
        public Affidavit Affidavit { get; set; } = null!;
        public bool CanAction { get; set; }
        public AffidavitStampDTO Stamp { get; set; } = new();
    }
}
