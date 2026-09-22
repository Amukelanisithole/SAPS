using System.ComponentModel.DataAnnotations;

namespace Logic.DTO
{
    public class AffidavitStampDTO
    {
        [Required(ErrorMessage = "Affidavit id is required")]
        public long AffidavitId { get; set; }

        [Required(ErrorMessage = "Force number is required")]
        [Display(Name = "Force Number")]
        public string ForceNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rank is required")]
        public string Rank { get; set; } = string.Empty;
    }
}
