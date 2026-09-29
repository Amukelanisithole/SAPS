using System.ComponentModel.DataAnnotations;

namespace Logic.DTO
{
    public class DocketDiaryEntryDTO
    {
        [Required(ErrorMessage = "Please choose the type of action")]
        [Display(Name = "Action")]
        public long EntryTypeId { get; set; }

        [Required(ErrorMessage = "Please enter when the action took place")]
        [Display(Name = "Date & Time")]
        public DateTime ActionDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Please describe what was done")]
        [MaxLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
        public string Notes { get; set; } = string.Empty;

        [Display(Name = "Share with complainant")]
        public bool SharedWithComplainant { get; set; }
    }
}
