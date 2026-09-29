using Database.AppDBContext;
using Database.Entities;
using Logic.BusinessLogicInterface;
using Logic.DTO;
using Logic.Model;
using Microsoft.EntityFrameworkCore;
using static Logic.Enums.Enums;

namespace Logic.BusinessLogic
{
    public class EmergencyAlertLogic : IEmergencyAlertLogic
    {
        private readonly ApplicationDbContext _context;
        private readonly IStationRoutingService _stationRoutingService;
        private readonly INotificationLogic _notificationLogic;
        private readonly IAuditLogWriter _auditLogWriter;

        public EmergencyAlertLogic(
            ApplicationDbContext context,
            IStationRoutingService stationRoutingService,
            INotificationLogic notificationLogic,
            IAuditLogWriter auditLogWriter)
        {
            _context = context;
            _stationRoutingService = stationRoutingService;
            _notificationLogic = notificationLogic;
            _auditLogWriter = auditLogWriter;
        }

        public async Task<ResultModel<EmergencyAlert>> SubmitAsync(long complainantId, EmergencyAlertDTO dto, string? ipAddress = null)
        {
            try
            {
                if (dto.Latitude == null || dto.Longitude == null)
                {
                    return new ResultModel<EmergencyAlert> { ResultId = EResultValue.Fail, Message = "Location access is required so this can be routed to the nearest station. Please enable location and try again." };
                }

                var nearestStationId = await _stationRoutingService.FindOrCreateNearestStationIdAsync(dto.Latitude.Value, dto.Longitude.Value);
                if (nearestStationId == null)
                {
                    return new ResultModel<EmergencyAlert> { ResultId = EResultValue.Fail, Message = "No SAPS station could be identified near your location — please call the emergency numbers above directly instead of waiting on this." };
                }

                var alert = new EmergencyAlert
                {
                    ComplainantId = complainantId,
                    Message = dto.Message,
                    Latitude = dto.Latitude.Value,
                    Longitude = dto.Longitude.Value,
                    NearestStationId = nearestStationId,
                    SubmissionTimestamp = DateTime.UtcNow,
                    StatusId = EEmergencyAlertStatus.New
                };

                _context.EmergencyAlerts.Add(alert);
                await _context.SaveChangesAsync();

                await _auditLogWriter.WriteAsync(new AuditLog
                {
                    EmergencyAlertId = alert.Id,
                    UserId = complainantId,
                    ActionType = "EmergencyAlertSubmitted",
                    Timestamp = DateTime.UtcNow,
                    IpAddress = ipAddress
                });

                var stationStaffIds = await _context.Users
                    .Where(u => u.StationId == nearestStationId && u.IsActive && (u.RoleId == ERole.SapsOfficer || u.RoleId == ERole.StationCommander))
                    .Select(u => u.Id)
                    .ToListAsync();

                foreach (var staffId in stationStaffIds)
                {
                    await _notificationLogic.CreateAsync(staffId, null,
                        "EMERGENCY ALERT: a complainant near your station has flagged an urgent situation. Open Emergency Alerts to review it now.");
                }

                return new ResultModel<EmergencyAlert> { ResultId = EResultValue.Success, Message = "Alert sent.", Data = alert };
            }
            catch (Exception ex)
            {
                return new ResultModel<EmergencyAlert> { ResultId = EResultValue.Fail, Message = $"An error occurred while sending the alert: {ex.Message}" };
            }
        }

        public async Task<EmergencyAlert?> GetByIdAsync(long id)
        {
            return await _context.EmergencyAlerts
                .Include(a => a.NavComplainant)
                .Include(a => a.NavNearestStation)
                .Include(a => a.NavAcknowledgedByOfficer)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<EmergencyAlert>> GetAllAsync()
        {
            return await _context.EmergencyAlerts
                .Include(a => a.NavComplainant)
                .Include(a => a.NavNearestStation)
                .OrderByDescending(a => a.SubmissionTimestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmergencyAlert>> GetForStationAsync(long stationId)
        {
            return await _context.EmergencyAlerts
                .Include(a => a.NavComplainant)
                .Where(a => a.NearestStationId == stationId && a.StatusId == EEmergencyAlertStatus.New)
                .OrderBy(a => a.SubmissionTimestamp)
                .ToListAsync();
        }

        public async Task<PagedResult<EmergencyAlert>> GetAllPagedAsync(int page, int pageSize, long? statusId = null, long? stationId = null, CancellationToken ct = default)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var query = _context.EmergencyAlerts
                .Include(a => a.NavComplainant)
                .Include(a => a.NavNearestStation)
                .AsQueryable();

            if (statusId != null)
            {
                query = query.Where(a => a.StatusId == statusId);
            }
            if (stationId != null)
            {
                query = query.Where(a => a.NearestStationId == stationId);
            }

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(a => a.SubmissionTimestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResult<EmergencyAlert> { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount };
        }

        public async Task<PagedResult<EmergencyAlert>> GetForStationPagedAsync(long stationId, int page, int pageSize, CancellationToken ct = default)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var query = _context.EmergencyAlerts
                .Include(a => a.NavComplainant)
                .Where(a => a.NearestStationId == stationId && a.StatusId == EEmergencyAlertStatus.New);

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderBy(a => a.SubmissionTimestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResult<EmergencyAlert> { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount };
        }

        public async Task<ResultModel<EmergencyAlert>> AcknowledgeAsync(long officerId, long alertId, string? ipAddress = null)
        {
            var alert = await _context.EmergencyAlerts.FirstOrDefaultAsync(a => a.Id == alertId);
            if (alert == null)
            {
                return new ResultModel<EmergencyAlert> { ResultId = EResultValue.Fail, Message = "Alert not found." };
            }

            if (alert.StatusId != EEmergencyAlertStatus.New)
            {
                return new ResultModel<EmergencyAlert> { ResultId = EResultValue.Fail, Message = "This alert has already been acknowledged." };
            }

            var officer = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == officerId);
            if (officer == null || officer.StationId != alert.NearestStationId)
            {
                return new ResultModel<EmergencyAlert> { ResultId = EResultValue.Fail, Message = "You may only acknowledge alerts routed to your own station." };
            }

            alert.StatusId = EEmergencyAlertStatus.Acknowledged;
            alert.AcknowledgedByOfficerId = officerId;
            alert.AcknowledgedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditLogWriter.WriteAsync(new AuditLog
            {
                EmergencyAlertId = alert.Id,
                UserId = officerId,
                ActionType = "EmergencyAlertAcknowledged",
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress
            });

            await _notificationLogic.CreateAsync(alert.ComplainantId, null,
                $"SAPS officer {officer.FullName} has acknowledged your emergency alert and is aware of your situation.");

            return new ResultModel<EmergencyAlert> { ResultId = EResultValue.Success, Message = "Alert acknowledged.", Data = alert };
        }
    }
}
