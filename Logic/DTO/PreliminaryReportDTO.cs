using System.ComponentModel.DataAnnotations;

namespace Logic.DTO
{
    public class PreliminaryReportDTO
    {
        [Required(ErrorMessage = "Incident type is required")]
        [Display(Name = "Incident Type")]
        public string IncidentType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Incident description is required")]
        [Display(Name = "Incident Description")]
        public string IncidentDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Incident date is required")]
        [Display(Name = "Incident Date")]
        public DateTime IncidentDate { get; set; }

        [Required(ErrorMessage = "Incident location is required")]
        [Display(Name = "Incident Location")]
        public string IncidentLocation { get; set; } = string.Empty;

        [Display(Name = "Supporting Information")]
        public string? SupportingInfo { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
