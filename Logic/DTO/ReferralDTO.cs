using System.ComponentModel.DataAnnotations;

namespace Logic.DTO
{
    public class ReferralDTO
    {
        [Required(ErrorMessage = "Docket id is required")]
        public long DocketId { get; set; }

        [Required(ErrorMessage = "Destination station id is required")]
        public long ToStationId { get; set; }

        [Required(ErrorMessage = "Reason is required")]
        public string Reason { get; set; } = string.Empty;
    }
}
