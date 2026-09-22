using System.ComponentModel.DataAnnotations;

namespace Logic.DTO
{
    public class DocketConvertDTO
    {
        [Required(ErrorMessage = "Preliminary report id is required")]
        public long ReportId { get; set; }

        [Display(Name = "CAS Number")]
        public string? CasNumber { get; set; }
    }
}
