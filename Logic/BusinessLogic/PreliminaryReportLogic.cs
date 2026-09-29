using Database.AppDBContext;
using Database.Entities;
using Logic.BusinessLogicInterface;
using Logic.DTO;
using Logic.Model;
using Microsoft.EntityFrameworkCore;
using static Logic.Enums.Enums;

namespace Logic.BusinessLogic
{
    public class PreliminaryReportLogic : IPreliminaryReportLogic
    {
        private readonly ApplicationDbContext _context;
        private readonly IStationRoutingService _stationRoutingService;
        private readonly INotificationLogic _notificationLogic;

        public PreliminaryReportLogic(ApplicationDbContext context, IStationRoutingService stationRoutingService, INotificationLogic notificationLogic)
        {
            _context = context;
            _stationRoutingService = stationRoutingService;
            _notificationLogic = notificationLogic;
        }

        public async Task<ResultModel<PreliminaryReport>> StartReviewAsync(long reportId, long officerId)
        {
            try
            {
                var (report, error) = await LoadForOfficerActionAsync(reportId, officerId);
                if (error != null) return error;

                if (report!.StatusId != EReportStatus.Submitted)
                {
                    return new ResultModel<PreliminaryReport> { ResultId = EResultValue.Fail, Message = $"This report is already {EReportStatus.GetName(report.StatusId)}." };
                }

                report.StatusId = EReportStatus.UnderReview;
                report.ReviewedByOfficerId = officerId;
                await _context.SaveChangesAsync();

                await _notificationLogic.CreateAsync(report.ComplainantId, null, $"Your report {report.ReferenceNumber} is now being reviewed by a SAPS officer.");

                return new ResultModel<PreliminaryReport> { ResultId = EResultValue.Success, Message = "Report marked as under review.", Data = report };
            }
            catch (Exception ex)
            {
                return new ResultModel<PreliminaryReport> { ResultId = EResultValue.Fail, Message = $"An error occurred while updating the report: {ex.Message}" };
            }
        }

        public async Task<ResultModel<PreliminaryReport>> RejectAsync(long reportId, long officerId, ReportRejectDTO dto)
        {
            try
            {
                var (report, error) = await LoadForOfficerActionAsync(reportId, officerId);
                if (error != null) return error;

                if (report!.StatusId is not (EReportStatus.Submitted or EReportStatus.UnderReview))
                {
                    return new ResultModel<PreliminaryReport> { ResultId = EResultValue.Fail, Message = $"A report that is {EReportStatus.GetName(report.StatusId)} cannot be rejected." };
                }

                if (string.IsNullOrWhiteSpace(dto.Reason))
                {
                    return new ResultModel<PreliminaryReport> { ResultId = EResultValue.Fail, Message = "Please give a reason for rejecting this report." };
                }

                report.StatusId = EReportStatus.Rejected;
                report.ReviewedByOfficerId = officerId;
                report.RejectionReason = dto.Reason.Trim();
                await _context.SaveChangesAsync();

                await _notificationLogic.CreateAsync(report.ComplainantId, null,
                    $"Your report {report.ReferenceNumber} was not accepted. Reason: {report.RejectionReason}");

                return new ResultModel<PreliminaryReport> { ResultId = EResultValue.Success, Message = "Report rejected and the complainant notified.", Data = report };
            }
            catch (Exception ex)
            {
                return new ResultModel<PreliminaryReport> { ResultId = EResultValue.Fail, Message = $"An error occurred while rejecting the report: {ex.Message}" };
            }
        }

        private async Task<(PreliminaryReport? Report, ResultModel<PreliminaryReport>? Error)> LoadForOfficerActionAsync(long reportId, long officerId)
        {
            var officer = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == officerId);
            var report = await _context.PreliminaryReports.FirstOrDefaultAsync(r => r.Id == reportId);

            if (report == null)
            {
                return (null, new ResultModel<PreliminaryReport> { ResultId = EResultValue.Fail, Message = "Preliminary report not found." });
            }

            if (officer?.StationId == null || (report.NearestStationId != null && report.NearestStationId != officer.StationId))
            {
                return (null, new ResultModel<PreliminaryReport> { ResultId = EResultValue.Fail, Message = "You may only action reports routed to your own station." });
            }

            return (report, null);
        }

        public async Task<ResultModel<PreliminaryReport>> SubmitAsync(long complainantId, PreliminaryReportDTO dto)
        {
            try
            {
                if (dto.Latitude == null || dto.Longitude == null)
                {
                    return new ResultModel<PreliminaryReport> { ResultId = EResultValue.Fail, Message = "Location access is required so your report can be routed to the nearest station. Please enable location and try again." };
                }

                var nearestStationId = await _stationRoutingService.FindOrCreateNearestStationIdAsync(dto.Latitude.Value, dto.Longitude.Value);
                if (nearestStationId == null)
                {
                    return new ResultModel<PreliminaryReport> { ResultId = EResultValue.Fail, Message = "No SAPS station could be found near your location. Please try again or contact an administrator." };
                }

                var report = new PreliminaryReport
                {
                    ComplainantId = complainantId,
                    ReferenceNumber = await GenerateUniqueReferenceNumberAsync(),
                    IncidentType = dto.IncidentType,
                    IncidentDescription = dto.IncidentDescription,
                    IncidentDate = dto.IncidentDate,
                    IncidentLocation = dto.IncidentLocation,
                    SupportingInfo = dto.SupportingInfo,
                    SubmissionTimestamp = DateTime.UtcNow,
                    StatusId = EReportStatus.Submitted,
                    ComplainantLatitude = dto.Latitude,
                    ComplainantLongitude = dto.Longitude,
                    NearestStationId = nearestStationId
                };

                _context.PreliminaryReports.Add(report);
                await _context.SaveChangesAsync();

                return new ResultModel<PreliminaryReport> { ResultId = EResultValue.Success, Message = "Report submitted successfully.", Data = report };
            }
            catch (Exception ex)
            {
                return new ResultModel<PreliminaryReport> { ResultId = EResultValue.Fail, Message = $"An error occurred while submitting the report: {ex.Message}" };
            }
        }

        public async Task<IEnumerable<PreliminaryReport>> GetUnderReviewByOfficerAsync(long officerId)
        {
            return await _context.PreliminaryReports
                .Where(r => r.StatusId == EReportStatus.UnderReview && r.ReviewedByOfficerId == officerId)
                .OrderBy(r => r.SubmissionTimestamp)
                .ToListAsync();
        }

        public async Task<PreliminaryReport?> GetByIdAsync(long id)
        {
            return await _context.PreliminaryReports
                .Include(r => r.NavDocket)
                .Include(r => r.NavNearestStation)
                .Include(r => r.NavReviewedByOfficer)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<PreliminaryReport>> GetMineAsync(long complainantId)
        {
            return await _context.PreliminaryReports
                .Where(r => r.ComplainantId == complainantId)
                .OrderByDescending(r => r.SubmissionTimestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<PreliminaryReport>> GetForReviewAsync(long stationId)
        {
            return await _context.PreliminaryReports
                .Where(r => (r.StatusId == EReportStatus.Submitted || r.StatusId == EReportStatus.UnderReview)
                            && r.NearestStationId == stationId)
                .OrderBy(r => r.SubmissionTimestamp)
                .ToListAsync();
        }

        public async Task<PagedResult<PreliminaryReport>> GetMinePagedAsync(long complainantId, int page, int pageSize, long? statusId = null, CancellationToken ct = default)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var query = _context.PreliminaryReports.Where(r => r.ComplainantId == complainantId);
            if (statusId != null)
            {
                query = query.Where(r => r.StatusId == statusId);
            }

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(r => r.SubmissionTimestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResult<PreliminaryReport> { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount };
        }

        public async Task<PagedResult<PreliminaryReport>> GetForReviewPagedAsync(long stationId, int page, int pageSize, long? statusId = null, CancellationToken ct = default)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var query = _context.PreliminaryReports
                .Include(r => r.NavReviewedByOfficer)
                .Where(r => (r.StatusId == EReportStatus.Submitted || r.StatusId == EReportStatus.UnderReview)
                            && r.NearestStationId == stationId);
            if (statusId != null)
            {
                query = query.Where(r => r.StatusId == statusId);
            }

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderBy(r => r.SubmissionTimestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResult<PreliminaryReport> { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount };
        }

        private async Task<string> GenerateUniqueReferenceNumberAsync()
        {
            string candidate;
            do
            {
                candidate = $"PR-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(100000, 999999)}";
            }
            while (await _context.PreliminaryReports.AnyAsync(r => r.ReferenceNumber == candidate));

            return candidate;
        }
    }
}
