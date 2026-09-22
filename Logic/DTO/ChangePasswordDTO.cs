using System.ComponentModel.DataAnnotations;
using Logic.Validation;

namespace Logic.DTO
{
    public class ChangePasswordDTO
    {
        [Required(ErrorMessage = "Your current password is required")]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "A new password is required")]
        [PasswordComplexity]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your new password")]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm New Password")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
