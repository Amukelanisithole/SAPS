using System.ComponentModel.DataAnnotations;

namespace Logic.DTO
{
    public class DocketCasNumberDTO
    {
        [Required(ErrorMessage = "CAS number is required")]
        [MaxLength(50, ErrorMessage = "CAS number cannot exceed 50 characters")]
        [Display(Name = "CAS Number")]
        public string CasNumber { get; set; } = string.Empty;
    }
}
