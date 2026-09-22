using Database.Entities;

namespace SAPS.Models
{
    public class ComplainantDashboardViewModel
    {
        public int ReportCount { get; set; }
        public int DocketCount { get; set; }
        public int AffidavitCount { get; set; }
        public int UnreadNotificationCount { get; set; }
        public IEnumerable<PreliminaryReport> RecentReports { get; set; } = Enumerable.Empty<PreliminaryReport>();
        public IEnumerable<Docket> RecentDockets { get; set; } = Enumerable.Empty<Docket>();
        public IEnumerable<Affidavit> RecentAffidavits { get; set; } = Enumerable.Empty<Affidavit>();
    }

    public class OfficerDashboardViewModel
    {
        public int ReportsAwaitingReviewCount { get; set; }
        public int DocketsAtMyStationCount { get; set; }
        public int AffidavitsAwaitingReviewCount { get; set; }
        public int ActiveEmergencyAlertCount { get; set; }
        public IEnumerable<PreliminaryReport> ReportsAwaitingReview { get; set; } = Enumerable.Empty<PreliminaryReport>();
        public IEnumerable<Docket> DocketsAtMyStation { get; set; } = Enumerable.Empty<Docket>();
        public IEnumerable<Affidavit> AffidavitsAwaitingReview { get; set; } = Enumerable.Empty<Affidavit>();
        public IEnumerable<EmergencyAlert> ActiveEmergencyAlerts { get; set; } = Enumerable.Empty<EmergencyAlert>();
    }

    public class CommanderDashboardViewModel
    {
        public int EscalationCount { get; set; }
        public int DocketsAtMyStationCount { get; set; }
        public int AffidavitsAwaitingReviewCount { get; set; }
        public int ActiveEmergencyAlertCount { get; set; }
        public IEnumerable<Escalation> RecentEscalations { get; set; } = Enumerable.Empty<Escalation>();
        public IEnumerable<Docket> DocketsAtMyStation { get; set; } = Enumerable.Empty<Docket>();
        public IEnumerable<Affidavit> AffidavitsAwaitingReview { get; set; } = Enumerable.Empty<Affidavit>();
        public IEnumerable<EmergencyAlert> ActiveEmergencyAlerts { get; set; } = Enumerable.Empty<EmergencyAlert>();
    }

    public class AdminDashboardViewModel
    {
        public int UserCount { get; set; }
        public int StationCount { get; set; }
        public int BackupCount { get; set; }
        public int AffidavitCount { get; set; }
        public int EmergencyAlertCount { get; set; }
        public IEnumerable<User> RecentUsers { get; set; } = Enumerable.Empty<User>();
        public IEnumerable<BackupRecord> RecentBackups { get; set; } = Enumerable.Empty<BackupRecord>();
    }
}
