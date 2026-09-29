using System.ComponentModel.DataAnnotations;

namespace Logic.DTO
{
    public class DocketClosureDTO
    {
        [Required(ErrorMessage = "Closure outcome is required")]
        [Display(Name = "Outcome")]
        public long ClosureOutcomeId { get; set; }

        [Required(ErrorMessage = "Closure notes are required")]
        [MaxLength(2000, ErrorMessage = "Closure notes cannot exceed 2000 characters")]
        [Display(Name = "Closure Notes")]
        public string ClosureNotes { get; set; } = string.Empty;
    }
}
