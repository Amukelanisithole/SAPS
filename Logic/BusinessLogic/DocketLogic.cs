using Database.AppDBContext;
using Database.Entities;
using Logic.BusinessLogicInterface;
using Logic.DTO;
using Logic.Model;
using Microsoft.EntityFrameworkCore;
using static Logic.Enums.Enums;

namespace Logic.BusinessLogic
{
    public class DocketLogic : IDocketLogic
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditLogWriter _auditLogWriter;
        private readonly INotificationLogic _notificationLogic;

        public DocketLogic(ApplicationDbContext context, IAuditLogWriter auditLogWriter, INotificationLogic notificationLogic)
        {
            _context = context;
            _auditLogWriter = auditLogWriter;
            _notificationLogic = notificationLogic;
        }

        // NFR-02: the docket always opens at the reviewing officer's own station —
        // it is derived from their account, never chosen freely by the caller.
        public async Task<ResultModel<Docket>> ConvertAsync(long officerId, DocketConvertDTO dto, string? ipAddress)
        {
            try
            {
                var officer = await _context.Users.FirstOrDefaultAsync(u => u.Id == officerId);
                if (officer?.StationId == null)
                {
                    return new ResultModel<Docket> { ResultId = EResultValue.Fail, Message = "Your account is not assigned to a station." };
                }

                var report = await _context.PreliminaryReports.FirstOrDefaultAsync(r => r.Id == dto.ReportId);
                if (report == null)
                {
                    return new ResultModel<Docket> { ResultId = EResultValue.Fail, Message = "Preliminary report not found." };
                }

                if (report.StatusId == EReportStatus.Converted)
                {
                    return new ResultModel<Docket> { ResultId = EResultValue.Fail, Message = "This report has already been converted into a docket." };
                }

                var docket = new Docket
                {
                    ReportId = report.Id,
                    StationId = officer.StationId.Value,
                    ReviewingOfficerId = officerId,
                    CasNumber = dto.CasNumber,
                    StatusId = EDocketStatus.Open,
                    OpenedDate = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                };

                _context.Dockets.Add(docket);
                report.StatusId = EReportStatus.Converted;
                await _context.SaveChangesAsync();

                await _auditLogWriter.WriteAsync(new AuditLog
                {
                    DocketId = docket.Id,
                    UserId = officerId,
                    ActionType = "Opened",
                    Timestamp = DateTime.UtcNow,
                    IpAddress = ipAddress
                });

                return new ResultModel<Docket> { ResultId = EResultValue.Success, Message = "Docket opened successfully.", Data = docket };
            }
            catch (Exception ex)
            {
                return new ResultModel<Docket> { ResultId = EResultValue.Fail, Message = $"An error occurred while opening the docket: {ex.Message}" };
            }
        }

        // FR-04: every open/view/update must be traceable to the accessing user, station and time.
        // NavReport is included so callers (e.g. the controller) can check whether the
        // viewing complainant actually owns this docket before returning it.
        public async Task<Docket?> GetByIdAsync(long id, long viewingUserId, string? ipAddress)
        {
            var docket = await _context.Dockets
                .Include(d => d.NavReport)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (docket == null) return null;

            await _auditLogWriter.WriteAsync(new AuditLog
            {
                DocketId = docket.Id,
                UserId = viewingUserId,
                ActionType = "Viewed",
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress
            });

            return docket;
        }

        public async Task<IEnumerable<Docket>> GetForComplainantAsync(long complainantId)
        {
            return await _context.Dockets
                .Include(d => d.NavReport)
                .Where(d => d.NavReport != null && d.NavReport.ComplainantId == complainantId)
                .OrderByDescending(d => d.LastUpdated)
                .ToListAsync();
        }

        // Lets an officer/commander find dockets at their own station without
        // already knowing a docket's id (e.g. from converting or referring it themselves).
        public async Task<IEnumerable<Docket>> GetForStationAsync(long stationId)
        {
            return await _context.Dockets
                .Where(d => d.StationId == stationId)
                .OrderByDescending(d => d.LastUpdated)
                .ToListAsync();
        }

        public async Task<PagedResult<Docket>> GetForComplainantPagedAsync(long complainantId, int page, int pageSize, long? statusId = null, CancellationToken ct = default)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var query = _context.Dockets
                .Include(d => d.NavReport)
                .Where(d => d.NavReport != null && d.NavReport.ComplainantId == complainantId);

            if (statusId != null)
            {
                query = query.Where(d => d.StatusId == statusId);
            }

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(d => d.LastUpdated)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResult<Docket> { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount };
        }

        public async Task<PagedResult<Docket>> GetForStationPagedAsync(long stationId, int page, int pageSize, long? statusId = null, CancellationToken ct = default)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var query = _context.Dockets.Where(d => d.StationId == stationId);

            if (statusId != null)
            {
                query = query.Where(d => d.StatusId == statusId);
            }

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(d => d.LastUpdated)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResult<Docket> { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount };
        }

        // NFR-02: only staff at the docket's own station may update it.
        public async Task<ResultModel<Docket>> UpdateStatusAsync(long docketId, long actingUserId, DocketStatusUpdateDTO dto, string? ipAddress)
        {
            try
            {
                var actingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == actingUserId);
                var docket = await _context.Dockets
                    .Include(d => d.NavReport)
                    .FirstOrDefaultAsync(d => d.Id == docketId);
                if (docket == null)
                {
                    return new ResultModel<Docket> { ResultId = EResultValue.Fail, Message = "Docket not found." };
                }

                if (actingUser?.StationId != docket.StationId)
                {
                    return new ResultModel<Docket> { ResultId = EResultValue.Fail, Message = "You may only update dockets held at your own station." };
                }

                docket.StatusId = dto.NewStatusId;
                docket.LastUpdated = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                await _auditLogWriter.WriteAsync(new AuditLog
                {
                    DocketId = docket.Id,
                    UserId = actingUserId,
                    ActionType = "Updated",
                    Timestamp = DateTime.UtcNow,
                    IpAddress = ipAddress
                });

                if (docket.NavReport != null)
                {
                    await _notificationLogic.CreateAsync(
                        docket.NavReport.ComplainantId,
                        docket.Id,
                        $"The status of your docket #{docket.Id} has been updated.");
                }

                return new ResultModel<Docket> { ResultId = EResultValue.Success, Message = "Docket status updated successfully.", Data = docket };
            }
            catch (Exception ex)
            {
                return new ResultModel<Docket> { ResultId = EResultValue.Fail, Message = $"An error occurred while updating the docket: {ex.Message}" };
            }
        }
    }
}
