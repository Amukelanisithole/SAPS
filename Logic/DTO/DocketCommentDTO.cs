using System.ComponentModel.DataAnnotations;

namespace Logic.DTO
{
    public class DocketCommentDTO
    {
        [Required]
        public long DocketId { get; set; }

        [Required(ErrorMessage = "Message cannot be empty")]
        [StringLength(2000, ErrorMessage = "Message is too long")]
        [Display(Name = "Message")]
        public string Message { get; set; } = string.Empty;
    }
}
