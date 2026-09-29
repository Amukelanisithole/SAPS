using System.ComponentModel.DataAnnotations;
using Logic.Validation;

namespace Logic.DTO
{
    public class UpdateUserDTO
    {
        [Required(ErrorMessage = "Full name is required")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [PhoneNumberDigits]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role id is required")]
        [Display(Name = "Role")]
        public long RoleId { get; set; }

        [Display(Name = "Station")]
        public long? StationId { get; set; }
    }
}
