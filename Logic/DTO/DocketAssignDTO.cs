using System.ComponentModel.DataAnnotations;

namespace Logic.DTO
{
    public class DocketAssignDTO
    {
        [Required(ErrorMessage = "Officer is required")]
        [Display(Name = "Investigating Officer")]
        public long OfficerId { get; set; }
    }
}
