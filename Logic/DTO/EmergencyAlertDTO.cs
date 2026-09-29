using System.ComponentModel.DataAnnotations;

namespace Logic.DTO
{
    public class EmergencyAlertDTO
    {
        [Display(Name = "What's happening?")]
        [MaxLength(500)]
        public string? Message { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
