using System.ComponentModel.DataAnnotations;

namespace Logic.DTO
{
    public class ReportRejectDTO
    {
        [Required(ErrorMessage = "Please give a reason for rejecting this report")]
        [MaxLength(1000, ErrorMessage = "Reason cannot exceed 1000 characters")]
        public string Reason { get; set; } = string.Empty;
    }
}
