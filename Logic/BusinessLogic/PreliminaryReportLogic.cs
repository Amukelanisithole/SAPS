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

        public PreliminaryReportLogic(ApplicationDbContext context, IStationRoutingService stationRoutingService)
        {
            _context = context;
            _stationRoutingService = stationRoutingService;
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

        public async Task<PreliminaryReport?> GetByIdAsync(long id)
        {
            return await _context.PreliminaryReports
                .Include(r => r.NavDocket)
                .Include(r => r.NavNearestStation)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<PreliminaryReport>> GetMineAsync(long complainantId)
        {
            return await _context.PreliminaryReports
                .Where(r => r.ComplainantId == complainantId)
                .OrderByDescending(r => r.SubmissionTimestamp)
                .ToListAsync();
        }

        // Routed reports only reach the officers at their nearest-matched station —
        // this is the actual "send that report to that station" behavior.
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

        // Reference numbers double as the complainant's proof of submission (FR-02),
        // so a collision would let one report silently shadow another's reference.
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
