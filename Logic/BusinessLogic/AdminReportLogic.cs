using Database.AppDBContext;
using Logic.BusinessLogicInterface;
using Logic.DTO;
using Microsoft.EntityFrameworkCore;
using static Logic.Enums.Enums;

namespace Logic.BusinessLogic
{
    public class AdminReportLogic : IAdminReportLogic
    {
        private readonly ApplicationDbContext _context;

        public AdminReportLogic(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ReportTableDTO?> GetReportAsync(string reportType)
        {
            return reportType switch
            {
                EReportExportType.Users => await GetUsersReportAsync(),
                EReportExportType.Stations => await GetStationsReportAsync(),
                EReportExportType.PreliminaryReports => await GetPreliminaryReportsReportAsync(),
                EReportExportType.Dockets => await GetDocketsReportAsync(),
                EReportExportType.Affidavits => await GetAffidavitsReportAsync(),
                EReportExportType.EmergencyAlerts => await GetEmergencyAlertsReportAsync(),
                EReportExportType.Escalations => await GetEscalationsReportAsync(),
                EReportExportType.Referrals => await GetReferralsReportAsync(),
                EReportExportType.AuditLogs => await GetAuditLogsReportAsync(),
                EReportExportType.Backups => await GetBackupsReportAsync(),
                _ => null
            };
        }

        private static string Fmt(DateTime dt) => dt.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
        private static string Fmt(DateTime? dt) => dt.HasValue ? Fmt(dt.Value) : "-";

        private async Task<ReportTableDTO> GetUsersReportAsync()
        {
            var users = await _context.Users.Include(u => u.NavStation).OrderBy(u => u.Id).ToListAsync();
            return new ReportTableDTO
            {
                Title = "Users",
                Headers = new[] { "Id", "Full Name", "Email", "Phone", "ID Number", "Role", "Station", "Active", "Created", "Last Login" },
                Rows = users.Select(u => new[]
                {
                    u.Id.ToString(), u.FullName, u.Email, u.PhoneNumber, u.IdNumber,
                    ERole.GetName(u.RoleId), u.NavStation?.StationName ?? "-",
                    u.IsActive ? "Yes" : "No", Fmt(u.CreatedDate), Fmt(u.LastLogin)
                }).ToList()
            };
        }

        private async Task<ReportTableDTO> GetStationsReportAsync()
        {
            var stations = await _context.Stations.OrderBy(s => s.StationName).ToListAsync();
            return new ReportTableDTO
            {
                Title = "Stations",
                Headers = new[] { "Id", "Name", "Province", "Address", "Contact Number", "Latitude", "Longitude" },
                Rows = stations.Select(s => new[]
                {
                    s.Id.ToString(), s.StationName, s.Province, s.Address, s.ContactNumber,
                    s.Latitude?.ToString("F5") ?? "-", s.Longitude?.ToString("F5") ?? "-"
                }).ToList()
            };
        }

        private async Task<ReportTableDTO> GetPreliminaryReportsReportAsync()
        {
            var reports = await _context.PreliminaryReports
                .Include(r => r.NavComplainant)
                .Include(r => r.NavNearestStation)
                .OrderByDescending(r => r.SubmissionTimestamp)
                .ToListAsync();
            return new ReportTableDTO
            {
                Title = "Preliminary Reports",
                Headers = new[] { "Id", "Reference Number", "Complainant", "Incident Type", "Location", "Nearest Station", "Status", "Submitted" },
                Rows = reports.Select(r => new[]
                {
                    r.Id.ToString(), r.ReferenceNumber, r.NavComplainant?.FullName ?? "-", r.IncidentType, r.IncidentLocation,
                    r.NavNearestStation?.StationName ?? "-", EReportStatus.GetName(r.StatusId), Fmt(r.SubmissionTimestamp)
                }).ToList()
            };
        }

        private async Task<ReportTableDTO> GetDocketsReportAsync()
        {
            var dockets = await _context.Dockets
                .Include(d => d.NavStation)
                .Include(d => d.NavReviewingOfficer)
                .OrderByDescending(d => d.OpenedDate)
                .ToListAsync();
            return new ReportTableDTO
            {
                Title = "Dockets",
                Headers = new[] { "Id", "CAS Number", "Station", "Reviewing Officer", "Status", "Opened", "Last Updated" },
                Rows = dockets.Select(d => new[]
                {
                    d.Id.ToString(), d.CasNumber ?? "-", d.NavStation?.StationName ?? "-", d.NavReviewingOfficer?.FullName ?? "-",
                    EDocketStatus.GetName(d.StatusId), Fmt(d.OpenedDate), Fmt(d.LastUpdated)
                }).ToList()
            };
        }

        private async Task<ReportTableDTO> GetAffidavitsReportAsync()
        {
            var affidavits = await _context.Affidavits
                .Include(a => a.NavComplainant)
                .Include(a => a.NavNearestStation)
                .Include(a => a.NavStampedByOfficer)
                .OrderByDescending(a => a.SubmissionTimestamp)
                .ToListAsync();
            return new ReportTableDTO
            {
                Title = "Affidavits",
                Headers = new[] { "Id", "Reference Number", "Complainant", "ID Number", "Station", "Status", "Certified By", "Submitted", "Certified" },
                Rows = affidavits.Select(a => new[]
                {
                    a.Id.ToString(), a.ReferenceNumber, a.NavComplainant?.FullName ?? a.FullName, a.IdNumber,
                    a.NavNearestStation?.StationName ?? "-", EAffidavitStatus.GetName(a.StatusId),
                    a.NavStampedByOfficer?.FullName ?? "-", Fmt(a.SubmissionTimestamp), Fmt(a.StampedDate)
                }).ToList()
            };
        }

        private async Task<ReportTableDTO> GetEmergencyAlertsReportAsync()
        {
            var alerts = await _context.EmergencyAlerts
                .Include(a => a.NavComplainant)
                .Include(a => a.NavNearestStation)
                .Include(a => a.NavAcknowledgedByOfficer)
                .OrderByDescending(a => a.SubmissionTimestamp)
                .ToListAsync();
            return new ReportTableDTO
            {
                Title = "Emergency Alerts",
                Headers = new[] { "Id", "Complainant", "Message", "Station", "Status", "Acknowledged By", "Submitted", "Acknowledged" },
                Rows = alerts.Select(a => new[]
                {
                    a.Id.ToString(), a.NavComplainant?.FullName ?? "-", a.Message ?? "-",
                    a.NavNearestStation?.StationName ?? "-", EEmergencyAlertStatus.GetName(a.StatusId),
                    a.NavAcknowledgedByOfficer?.FullName ?? "-", Fmt(a.SubmissionTimestamp), Fmt(a.AcknowledgedDate)
                }).ToList()
            };
        }

        private async Task<ReportTableDTO> GetEscalationsReportAsync()
        {
            var escalations = await _context.Escalations
                .Include(e => e.NavComplainant)
                .OrderByDescending(e => e.EscalationDate)
                .ToListAsync();
            return new ReportTableDTO
            {
                Title = "Escalations",
                Headers = new[] { "Id", "Docket", "Complainant", "Description", "Status", "Raised" },
                Rows = escalations.Select(e => new[]
                {
                    e.Id.ToString(), $"#{e.DocketId}", e.NavComplainant?.FullName ?? "-", e.Description,
                    EEscalationStatus.GetName(e.StatusId), Fmt(e.EscalationDate)
                }).ToList()
            };
        }

        private async Task<ReportTableDTO> GetReferralsReportAsync()
        {
            var referrals = await _context.Referrals
                .Include(r => r.NavFromStation)
                .Include(r => r.NavToStation)
                .Include(r => r.NavReferredByOfficer)
                .OrderByDescending(r => r.ReferralDate)
                .ToListAsync();
            return new ReportTableDTO
            {
                Title = "Referrals",
                Headers = new[] { "Id", "Docket", "From Station", "To Station", "Referred By", "Reason", "Date" },
                Rows = referrals.Select(r => new[]
                {
                    r.Id.ToString(), $"#{r.DocketId}", r.NavFromStation?.StationName ?? "-", r.NavToStation?.StationName ?? "-",
                    r.NavReferredByOfficer?.FullName ?? "-", r.Reason, Fmt(r.ReferralDate)
                }).ToList()
            };
        }

        private async Task<ReportTableDTO> GetAuditLogsReportAsync()
        {
            var logs = await _context.AuditLogs
                .Include(a => a.NavUser)
                .Include(a => a.NavAffidavit)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
            return new ReportTableDTO
            {
                Title = "Audit Logs",
                Headers = new[] { "Id", "Timestamp", "Action", "User", "Docket", "Affidavit", "Emergency Alert", "IP Address" },
                Rows = logs.Select(a => new[]
                {
                    a.Id.ToString(), Fmt(a.Timestamp), a.ActionType, a.NavUser?.FullName ?? "-",
                    a.DocketId.HasValue ? $"#{a.DocketId}" : "-", a.NavAffidavit?.ReferenceNumber ?? "-",
                    a.EmergencyAlertId.HasValue ? $"#{a.EmergencyAlertId}" : "-", a.IpAddress ?? "-"
                }).ToList()
            };
        }

        private async Task<ReportTableDTO> GetBackupsReportAsync()
        {
            var backups = await _context.BackupRecords
                .Include(b => b.NavPerformedByUser)
                .OrderByDescending(b => b.BackupDate)
                .ToListAsync();
            return new ReportTableDTO
            {
                Title = "Backup Records",
                Headers = new[] { "Id", "Date", "Status", "Performed By", "Restoration Tested" },
                Rows = backups.Select(b => new[]
                {
                    b.Id.ToString(), Fmt(b.BackupDate), EBackupStatus.GetName(b.StatusId),
                    b.NavPerformedByUser?.FullName ?? "-", b.RestorationTested ? "Yes" : "No"
                }).ToList()
            };
        }
    }
}
