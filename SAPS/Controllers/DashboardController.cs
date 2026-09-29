using Logic.BusinessLogicInterface;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPS.Extensions;
using SAPS.Models;
using static Logic.Enums.Enums;

namespace SAPS.Controllers
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class DashboardController : Controller
    {
        private readonly IPreliminaryReportLogic _preliminaryReportLogic;
        private readonly IDocketLogic _docketLogic;
        private readonly IEscalationLogic _escalationLogic;
        private readonly INotificationLogic _notificationLogic;
        private readonly IUserManagementLogic _userManagementLogic;
        private readonly IStationLogic _stationLogic;
        private readonly IBackupLogic _backupLogic;
        private readonly IAffidavitLogic _affidavitLogic;
        private readonly IEmergencyAlertLogic _emergencyAlertLogic;
        private readonly IReferralLogic _referralLogic;
        private readonly IDocketEvidenceLogic _docketEvidenceLogic;
        private readonly Logic.Model.DocketSettings _docketSettings;

        public DashboardController(
            IPreliminaryReportLogic preliminaryReportLogic,
            IDocketLogic docketLogic,
            IEscalationLogic escalationLogic,
            INotificationLogic notificationLogic,
            IUserManagementLogic userManagementLogic,
            IStationLogic stationLogic,
            IBackupLogic backupLogic,
            IAffidavitLogic affidavitLogic,
            IEmergencyAlertLogic emergencyAlertLogic,
            IReferralLogic referralLogic,
            IDocketEvidenceLogic docketEvidenceLogic,
            Microsoft.Extensions.Options.IOptions<Logic.Model.DocketSettings> docketSettings)
        {
            _referralLogic = referralLogic;
            _docketEvidenceLogic = docketEvidenceLogic;
            _docketSettings = docketSettings.Value;
            _preliminaryReportLogic = preliminaryReportLogic;
            _docketLogic = docketLogic;
            _escalationLogic = escalationLogic;
            _notificationLogic = notificationLogic;
            _userManagementLogic = userManagementLogic;
            _stationLogic = stationLogic;
            _backupLogic = backupLogic;
            _affidavitLogic = affidavitLogic;
            _emergencyAlertLogic = emergencyAlertLogic;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.GetUserId();

            switch (User.GetRoleName())
            {
                case "Complainant":
                    {
                        var reports = (await _preliminaryReportLogic.GetMineAsync(userId)).ToList();
                        var dockets = (await _docketLogic.GetForComplainantAsync(userId)).ToList();
                        var notifications = (await _notificationLogic.GetMineAsync(userId)).ToList();
                        var affidavits = (await _affidavitLogic.GetMineAsync(userId)).ToList();
                        return View("Complainant", new ComplainantDashboardViewModel
                        {
                            ReportCount = reports.Count,
                            DocketCount = dockets.Count,
                            AffidavitCount = affidavits.Count,
                            UnreadNotificationCount = notifications.Count(n => !n.ReadStatus),
                            RecentReports = reports.Take(5),
                            RecentDockets = dockets.Take(5),
                            RecentAffidavits = affidavits.Take(5)
                        });
                    }

                case "SapsOfficer":
                    {
                        var stationId = User.GetStationId();
                        var reportsAwaitingReview = stationId == null
                            ? new List<Database.Entities.PreliminaryReport>()
                            : (await _preliminaryReportLogic.GetForReviewAsync(stationId.Value)).ToList();
                        var docketsAtStation = stationId == null
                            ? new List<Database.Entities.Docket>()
                            : (await _docketLogic.GetForStationAsync(stationId.Value)).ToList();
                        var affidavitsAwaitingReview = stationId == null
                            ? new List<Database.Entities.Affidavit>()
                            : (await _affidavitLogic.GetForReviewAsync(stationId.Value)).ToList();
                        var activeEmergencyAlerts = stationId == null
                            ? new List<Database.Entities.EmergencyAlert>()
                            : (await _emergencyAlertLogic.GetForStationAsync(stationId.Value)).ToList();
                        var overdueDays = _docketSettings.FeedbackOverdueDays;
                        var myDockets = (await _docketLogic.GetAssignedPagedAsync(userId, 1, 200, includeFinished: false)).Items;
                        return View("Officer", new OfficerDashboardViewModel
                        {
                            MyActiveDocketCount = myDockets.Count,
                            MyUpdatesDueCount = myDockets.Count(d => d.Docket.StatusId is (EDocketStatus.Open or EDocketStatus.UnderInvestigation)
                                && (DateTime.UtcNow - d.LastActivity).TotalDays >= overdueDays),
                            MyReportsInReviewCount = (await _preliminaryReportLogic.GetUnderReviewByOfficerAsync(userId)).Count(),
                            MyEvidenceToReviewCount = await _docketEvidenceLogic.CountPendingForInvestigatorAsync(userId),
                            OverdueThresholdDays = overdueDays,
                            MyDockets = myDockets.Take(5),
                            ReportsAwaitingReviewCount = reportsAwaitingReview.Count,
                            DocketsAtMyStationCount = docketsAtStation.Count,
                            AffidavitsAwaitingReviewCount = affidavitsAwaitingReview.Count,
                            ActiveEmergencyAlertCount = activeEmergencyAlerts.Count,
                            ReportsAwaitingReview = reportsAwaitingReview.Take(5),
                            DocketsAtMyStation = docketsAtStation.Take(5),
                            AffidavitsAwaitingReview = affidavitsAwaitingReview.Take(5),
                            ActiveEmergencyAlerts = activeEmergencyAlerts.Take(5)
                        });
                    }

                case "StationCommander":
                    {
                        var stationId = User.GetStationId();
                        var escalations = stationId == null
                            ? new List<Database.Entities.Escalation>()
                            : (await _escalationLogic.GetForStationAsync(stationId.Value)).ToList();
                        var docketsAtStation = stationId == null
                            ? new List<Database.Entities.Docket>()
                            : (await _docketLogic.GetForStationAsync(stationId.Value)).ToList();
                        var affidavitsAwaitingReview = stationId == null
                            ? new List<Database.Entities.Affidavit>()
                            : (await _affidavitLogic.GetForReviewAsync(stationId.Value)).ToList();
                        var activeEmergencyAlerts = stationId == null
                            ? new List<Database.Entities.EmergencyAlert>()
                            : (await _emergencyAlertLogic.GetForStationAsync(stationId.Value)).ToList();
                        var commanderOverdueDays = _docketSettings.FeedbackOverdueDays;
                        return View("Commander", new CommanderDashboardViewModel
                        {
                            WorkQueue = stationId == null
                                ? new Logic.Model.StationWorkQueue()
                                : await _docketLogic.GetStationWorkQueueAsync(stationId.Value, commanderOverdueDays),
                            IncomingReferrals = stationId == null
                                ? Enumerable.Empty<Database.Entities.Referral>()
                                : await _referralLogic.GetPendingIncomingAsync(stationId.Value),
                            OverdueThresholdDays = commanderOverdueDays,
                            EscalationCount = escalations.Count,
                            DocketsAtMyStationCount = docketsAtStation.Count,
                            AffidavitsAwaitingReviewCount = affidavitsAwaitingReview.Count,
                            ActiveEmergencyAlertCount = activeEmergencyAlerts.Count,
                            RecentEscalations = escalations.Take(5),
                            DocketsAtMyStation = docketsAtStation.Take(5),
                            AffidavitsAwaitingReview = affidavitsAwaitingReview.Take(5),
                            ActiveEmergencyAlerts = activeEmergencyAlerts.Take(5)
                        });
                    }

                case "SystemAdministrator":
                    {
                        var users = (await _userManagementLogic.GetAllAsync()).ToList();
                        var stations = (await _stationLogic.GetAllAsync()).ToList();
                        var backups = (await _backupLogic.GetAllAsync()).ToList();
                        var affidavits = (await _affidavitLogic.GetAllAsync()).ToList();
                        var emergencyAlerts = (await _emergencyAlertLogic.GetAllAsync()).ToList();
                        return View("Admin", new AdminDashboardViewModel
                        {
                            UserCount = users.Count,
                            StationCount = stations.Count,
                            BackupCount = backups.Count,
                            AffidavitCount = affidavits.Count,
                            EmergencyAlertCount = emergencyAlerts.Count,
                            RecentUsers = users.Take(5),
                            RecentBackups = backups.Take(5)
                        });
                    }

                default:
                    return Forbid();
            }
        }
    }
}
