using System.ComponentModel.DataAnnotations;

namespace Logic.DTO
{
    public class DocketStatusUpdateDTO
    {
        [Required(ErrorMessage = "New status id is required")]
        public long NewStatusId { get; set; }

        [MaxLength(1000, ErrorMessage = "Reason cannot exceed 1000 characters")]
        public string? Reason { get; set; }
    }
}
