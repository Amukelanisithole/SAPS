using System.ComponentModel.DataAnnotations;

namespace Logic.DTO
{
    public class EscalationStatusUpdateDTO
    {
        [Required(ErrorMessage = "New status id is required")]
        public long NewStatusId { get; set; }
    }
}
