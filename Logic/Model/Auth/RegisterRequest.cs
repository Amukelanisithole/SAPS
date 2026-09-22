using System.ComponentModel.DataAnnotations;
using Logic.Validation;

namespace Logic.Model.Auth
{
    // Self-registration is for complainants only — SAPS Officer, Station
    // Commander and System Administrator accounts are provisioned internally,
    // not opened to the public via this endpoint.
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Full name is required")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "ID number is required")]
        [IdNumber]
        [Display(Name = "ID Number")]
        public string IdNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [PasswordComplexity]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password")]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [PhoneNumberDigits]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
