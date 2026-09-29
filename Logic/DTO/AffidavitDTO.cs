using System.ComponentModel.DataAnnotations;
using Logic.Validation;

namespace Logic.DTO
{
    public class AffidavitDTO
    {
        [Required(ErrorMessage = "Full name is required")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "ID number is required")]
        [IdNumber]
        [Display(Name = "ID Number")]
        public string IdNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Age is required")]
        [Range(1, 120, ErrorMessage = "Enter a valid age")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Working address is required")]
        [Display(Name = "Working Address")]
        public string WorkingAddress { get; set; } = string.Empty;

        [PhoneNumberDigits]
        [Display(Name = "Telephone (Work)")]
        public string? TelWork { get; set; }

        [PhoneNumberDigits]
        [Display(Name = "Telephone (Home)")]
        public string? TelHome { get; set; }

        [Required(ErrorMessage = "A cellphone number is required")]
        [PhoneNumberDigits]
        [Display(Name = "Cellphone")]
        public string TelCell { get; set; } = string.Empty;

        [Required(ErrorMessage = "Your declaration/statement is required")]
        [Display(Name = "Declaration / Statement")]
        public string DeclarationText { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please indicate whether you object to taking the prescribed oath")]
        [Display(Name = "Objection to the prescribed oath")]
        public bool? ObjectionToOath { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public byte[]? VerificationPhotoBytes { get; set; }
        public string? VerificationPhotoFileName { get; set; }

        public byte[]? IdDocumentBytes { get; set; }
        public string? IdDocumentFileName { get; set; }
        public string? IdDocumentContentType { get; set; }
    }
}
