using System.ComponentModel.DataAnnotations;

namespace Logic.DTO
{
    public class EscalationDTO
    {
        [Required(ErrorMessage = "Docket id is required")]
        public long DocketId { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = string.Empty;
    }
}
