using System.ComponentModel.DataAnnotations;
using Logic.Validation;

namespace Logic.DTO
{
    public class StationDTO
    {
        [Required(ErrorMessage = "Station name is required")]
        [Display(Name = "Station Name")]
        public string StationName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Province is required")]
        [Display(Name = "Province")]
        public string Province { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact number is required")]
        [PhoneNumberDigits]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; } = string.Empty;

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
