using System.ComponentModel.DataAnnotations;

namespace Logic.DTO
{
    public class EmergencyAlertDTO
    {
        [Display(Name = "What's happening?")]
        [MaxLength(500)]
        public string? Message { get; set; }

        // Captured client-side via the browser Geolocation API, same as
        // PreliminaryReportDTO/AffidavitDTO — required so the alert can be
        // routed to a station's review queue.
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
