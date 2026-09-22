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

        // Nullable so the form can force an explicit choice — a plain checkbox
        // left untouched would silently submit as "no objection", which isn't
        // the same thing as the deponent actually having made that choice.
        [Required(ErrorMessage = "Please indicate whether you object to taking the prescribed oath")]
        [Display(Name = "Objection to the prescribed oath")]
        public bool? ObjectionToOath { get; set; }

        // Captured client-side via the browser Geolocation API, same as
        // PreliminaryReportDTO — required so the affidavit can be routed to
        // a station's review queue.
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // Populated by the controller from the uploaded IFormFile before this
        // DTO reaches the Logic layer, which stays free of ASP.NET Core types.
        public byte[]? VerificationPhotoBytes { get; set; }
        public string? VerificationPhotoFileName { get; set; }

        // A photo/scan of the deponent's ID document — like VerificationPhotoBytes,
        // populated by the controller post-binding, so required-ness is enforced
        // there and in AffidavitLogic.SubmitAsync rather than via a DataAnnotation.
        public byte[]? IdDocumentBytes { get; set; }
        public string? IdDocumentFileName { get; set; }
        public string? IdDocumentContentType { get; set; }
    }
}
