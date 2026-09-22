using Database.AppDBContext;
using Database.Entities;
using Logic.BusinessLogicInterface;
using Logic.DTO;
using Logic.Model;
using Microsoft.EntityFrameworkCore;
using static Logic.Enums.Enums;

namespace Logic.BusinessLogic
{
    public class EscalationLogic : IEscalationLogic
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditLogWriter _auditLogWriter;
        private readonly INotificationLogic _notificationLogic;

        public EscalationLogic(ApplicationDbContext context, IAuditLogWriter auditLogWriter, INotificationLogic notificationLogic)
        {
            _context = context;
            _auditLogWriter = auditLogWriter;
            _notificationLogic = notificationLogic;
        }

        public async Task<ResultModel<Escalation>> CreateAsync(long complainantId, EscalationDTO dto)
        {
            try
            {
                var docket = await _context.Dockets
                    .Include(d => d.NavReport)
                    .FirstOrDefaultAsync(d => d.Id == dto.DocketId);
                if (docket == null)
                {
                    return new ResultModel<Escalation> { ResultId = EResultValue.Fail, Message = "Docket not found." };
                }

                if (docket.NavReport == null || docket.NavReport.ComplainantId != complainantId)
                {
                    return new ResultModel<Escalation> { ResultId = EResultValue.Fail, Message = "You may only escalate matters relating to your own case." };
                }

                var escalation = new Escalation
                {
                    DocketId = docket.Id,
                    ComplainantId = complainantId,
                    Description = dto.Description,
                    EscalationDate = DateTime.UtcNow,
                    StatusId = EEscalationStatus.Open
                };

                _context.Escalations.Add(escalation);
                await _context.SaveChangesAsync();

                await _auditLogWriter.WriteAsync(new AuditLog
                {
                    DocketId = docket.Id,
                    UserId = complainantId,
                    ActionType = "Escalated",
                    Timestamp = DateTime.UtcNow
                });

                // FR-09: Station Commanders at the docket's current station must be
                // notified so escalated/unresolved cases can't quietly go unseen.
                var commanderIds = await _context.Users
                    .Where(u => u.RoleId == ERole.StationCommander && u.StationId == docket.StationId)
                    .Select(u => u.Id)
                    .ToListAsync();

                foreach (var commanderId in commanderIds)
                {
                    await _notificationLogic.CreateAsync(
                        commanderId,
                        docket.Id,
                        $"A new escalation was lodged for docket #{docket.Id}.");
                }

                return new ResultModel<Escalation> { ResultId = EResultValue.Success, Message = "Escalation lodged successfully.", Data = escalation };
            }
            catch (Exception ex)
            {
                return new ResultModel<Escalation> { ResultId = EResultValue.Fail, Message = $"An error occurred while lodging the escalation: {ex.Message}" };
            }
        }

        public async Task<IEnumerable<Escalation>> GetMineAsync(long complainantId)
        {
            return await _context.Escalations
                .Where(e => e.ComplainantId == complainantId)
                .OrderByDescending(e => e.EscalationDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Escalation>> GetForStationAsync(long stationId)
        {
            return await _context.Escalations
                .Include(e => e.NavDocket)
                .Where(e => e.NavDocket != null && e.NavDocket.StationId == stationId)
                .OrderByDescending(e => e.EscalationDate)
                .ToListAsync();
        }

        public async Task<PagedResult<Escalation>> GetMinePagedAsync(long complainantId, int page, int pageSize, long? statusId = null, CancellationToken ct = default)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var query = _context.Escalations.Where(e => e.ComplainantId == complainantId);
            if (statusId != null)
            {
                query = query.Where(e => e.StatusId == statusId);
            }

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(e => e.EscalationDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResult<Escalation> { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount };
        }

        public async Task<PagedResult<Escalation>> GetForStationPagedAsync(long stationId, int page, int pageSize, long? statusId = null, CancellationToken ct = default)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var query = _context.Escalations
                .Include(e => e.NavDocket)
                .Where(e => e.NavDocket != null && e.NavDocket.StationId == stationId);
            if (statusId != null)
            {
                query = query.Where(e => e.StatusId == statusId);
            }

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(e => e.EscalationDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResult<Escalation> { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount };
        }

        public async Task<IEnumerable<Escalation>> GetByDocketIdAsync(long docketId)
        {
            return await _context.Escalations
                .Where(e => e.DocketId == docketId)
                .OrderByDescending(e => e.EscalationDate)
                .ToListAsync();
        }

        public async Task<ResultModel<Escalation>> UpdateStatusAsync(long escalationId, long commanderId, EscalationStatusUpdateDTO dto)
        {
            try
            {
                var commander = await _context.Users.FirstOrDefaultAsync(u => u.Id == commanderId);
                var escalation = await _context.Escalations
                    .Include(e => e.NavDocket)
                    .FirstOrDefaultAsync(e => e.Id == escalationId);
                if (escalation == null)
                {
                    return new ResultModel<Escalation> { ResultId = EResultValue.Fail, Message = "Escalation not found." };
                }

                // NFR-02: a commander only resolves escalations raised against their own station.
                if (escalation.NavDocket == null || commander?.StationId != escalation.NavDocket.StationId)
                {
                    return new ResultModel<Escalation> { ResultId = EResultValue.Fail, Message = "You may only update escalations at your own station." };
                }

                escalation.StatusId = dto.NewStatusId;
                await _context.SaveChangesAsync();

                await _auditLogWriter.WriteAsync(new AuditLog
                {
                    DocketId = escalation.DocketId,
                    UserId = commanderId,
                    ActionType = "EscalationStatusUpdated",
                    Timestamp = DateTime.UtcNow
                });

                // The complainant is notified for every other case event (status
                // changes, referrals, new messages) — resolution of the escalation
                // they themselves raised is the one they'd most want to know about.
                await _notificationLogic.CreateAsync(
                    escalation.ComplainantId,
                    escalation.DocketId,
                    $"Your escalation on docket #{escalation.DocketId} is now {EEscalationStatus.GetName(escalation.StatusId)}.");

                return new ResultModel<Escalation> { ResultId = EResultValue.Success, Message = "Escalation status updated successfully.", Data = escalation };
            }
            catch (Exception ex)
            {
                return new ResultModel<Escalation> { ResultId = EResultValue.Fail, Message = $"An error occurred while updating the escalation: {ex.Message}" };
            }
        }
    }
}
