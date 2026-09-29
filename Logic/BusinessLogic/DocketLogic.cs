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

                if (report.StatusId == EReportStatus.Rejected)
                {
                    return new ResultModel<Docket> { ResultId = EResultValue.Fail, Message = "This report has been rejected and cannot be converted into a docket." };
                }

                if (report.NearestStationId != null && report.NearestStationId != officer.StationId)
                {
                    return new ResultModel<Docket> { ResultId = EResultValue.Fail, Message = "You may only open dockets for reports routed to your own station." };
                }

                var casNumber = string.IsNullOrWhiteSpace(dto.CasNumber) ? null : dto.CasNumber.Trim();
                if (casNumber != null && await _context.Dockets.AnyAsync(d => d.CasNumber == casNumber))
                {
                    return new ResultModel<Docket> { ResultId = EResultValue.Fail, Message = $"CAS number {casNumber} is already assigned to another docket." };
                }

                var docket = new Docket
                {
                    ReportId = report.Id,
                    StationId = officer.StationId.Value,
                    ReviewingOfficerId = officerId,
                    InvestigatingOfficerId = officerId,
                    CasNumber = casNumber,
                    StatusId = EDocketStatus.Open,
                    OpenedDate = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                };
                docket.NavStatusHistory.Add(NewHistory(null, EDocketStatus.Open, officerId, "Docket opened from preliminary report."));

                _context.Dockets.Add(docket);
                report.StatusId = EReportStatus.Converted;
                report.ReviewedByOfficerId ??= officerId;
                await _context.SaveChangesAsync();

                await WriteAuditAsync(docket.Id, officerId, "Opened", ipAddress);

                await _notificationLogic.CreateAsync(
                    report.ComplainantId,
                    docket.Id,
                    $"Your report {report.ReferenceNumber} has been accepted and opened as docket #{docket.Id}.");

                return new ResultModel<Docket> { ResultId = EResultValue.Success, Message = "Docket opened successfully.", Data = docket };
            }
            catch (Exception ex)
            {
                return new ResultModel<Docket> { ResultId = EResultValue.Fail, Message = $"An error occurred while opening the docket: {ex.Message}" };
            }
        }

        public async Task<Docket?> GetByIdAsync(long id, long viewingUserId, string? ipAddress, bool includeDetails = false)
        {
            var query = _context.Dockets.Include(d => d.NavReport).AsQueryable();
            if (includeDetails)
            {
                query = query
                    .Include(d => d.NavStation)
                    .Include(d => d.NavReviewingOfficer)
                    .Include(d => d.NavInvestigatingOfficer)
                    .Include(d => d.NavClosedBy);
            }

            var docket = await query.FirstOrDefaultAsync(d => d.Id == id);
            if (docket == null) return null;

            await WriteAuditAsync(docket.Id, viewingUserId, "Viewed", ipAddress);

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

        public async Task<PagedResult<AssignedDocketItem>> GetAssignedPagedAsync(long officerId, int page, int pageSize, bool includeFinished, CancellationToken ct = default)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var query = _context.Dockets
                .Include(d => d.NavReport)
                .Where(d => d.InvestigatingOfficerId == officerId);

            if (!includeFinished)
            {
                query = query.Where(d => d.StatusId != EDocketStatus.Closed && d.StatusId != EDocketStatus.Archived);
            }

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .Select(d => new AssignedDocketItem
                {
                    Docket = d,
                    LastActivity = d.NavDiaryEntries.Max(e => (DateTime?)e.CreatedDate) ?? d.OpenedDate
                })
                .OrderBy(x => x.Docket.StatusId == EDocketStatus.Closed || x.Docket.StatusId == EDocketStatus.Archived)
                .ThenBy(x => x.LastActivity)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResult<AssignedDocketItem> { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount };
        }

        public async Task<StationWorkQueue> GetStationWorkQueueAsync(long stationId, int overdueDays, CancellationToken ct = default)
        {
            var activeAtStation = _context.Dockets
                .Include(d => d.NavReport)
                .Include(d => d.NavInvestigatingOfficer)
                .Where(d => d.StationId == stationId
                    && (d.StatusId == EDocketStatus.Open || d.StatusId == EDocketStatus.UnderInvestigation));

            var cutoff = DateTime.UtcNow.AddDays(-overdueDays);

            return new StationWorkQueue
            {
                PendingClosure = await _context.Dockets
                    .Include(d => d.NavReport)
                    .Include(d => d.NavInvestigatingOfficer)
                    .Where(d => d.StationId == stationId && d.StatusId == EDocketStatus.PendingClosure)
                    .OrderBy(d => d.LastUpdated)
                    .ToListAsync(ct),
                Unassigned = await activeAtStation
                    .Where(d => d.InvestigatingOfficerId == null)
                    .OrderBy(d => d.OpenedDate)
                    .ToListAsync(ct),
                Overdue = await activeAtStation
                    .Select(d => new AssignedDocketItem
                    {
                        Docket = d,
                        LastActivity = d.NavDiaryEntries.Max(e => (DateTime?)e.CreatedDate) ?? d.OpenedDate
                    })
                    .Where(x => x.LastActivity < cutoff)
                    .OrderBy(x => x.LastActivity)
                    .ToListAsync(ct)
            };
        }

        public async Task<IEnumerable<DocketStatusHistory>> GetStatusHistoryAsync(long docketId)
        {
            return await _context.DocketStatusHistory
                .Include(h => h.NavChangedBy)
                .Where(h => h.DocketId == docketId)
                .OrderBy(h => h.ChangedDate)
                .ThenBy(h => h.Id)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetAssignableOfficersAsync(long stationId)
        {
            return await _context.Users
                .Where(u => u.StationId == stationId && u.RoleId == ERole.SapsOfficer && u.IsActive)
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        public async Task<ResultModel<Docket>> UpdateStatusAsync(long docketId, long actingUserId, DocketStatusUpdateDTO dto, string? ipAddress)
        {
            try
            {
                var (docket, actingUser, error) = await LoadForStaffActionAsync(docketId, actingUserId);
                if (error != null) return error;

                var isCommander = actingUser!.RoleId == ERole.StationCommander;
                var fromStatusId = docket!.StatusId;
                if (!EDocketStatus.GetManualTransitions(fromStatusId, isCommander).Contains(dto.NewStatusId))
                {
                    return Fail($"A docket cannot be moved from {EDocketStatus.GetName(fromStatusId)} to {EDocketStatus.GetName(dto.NewStatusId)}{(isCommander ? "" : " by an officer")}.");
                }

                var reason = string.IsNullOrWhiteSpace(dto.Reason) ? null : dto.Reason.Trim();
                if (reason == null && EDocketStatus.RequiresReason(fromStatusId))
                {
                    return Fail("A reason is required for this status change.");
                }

                if (fromStatusId is EDocketStatus.PendingClosure or EDocketStatus.Closed && dto.NewStatusId == EDocketStatus.UnderInvestigation)
                {
                    docket.ClosureOutcomeId = null;
                    docket.ClosureNotes = null;
                    docket.ClosedDate = null;
                    docket.ClosedByUserId = null;
                }

                docket.StatusId = dto.NewStatusId;
                docket.LastUpdated = DateTime.UtcNow;
                docket.NavStatusHistory.Add(NewHistory(fromStatusId, dto.NewStatusId, actingUserId, reason));
                await _context.SaveChangesAsync();

                await WriteAuditAsync(docket.Id, actingUserId, $"Status: {EDocketStatus.GetName(fromStatusId)} -> {EDocketStatus.GetName(dto.NewStatusId)}", ipAddress);

                if (fromStatusId == EDocketStatus.PendingClosure)
                {
                    if (docket.InvestigatingOfficerId != null)
                    {
                        await _notificationLogic.CreateAsync(docket.InvestigatingOfficerId.Value, docket.Id,
                            $"The closure request for docket #{docket.Id} was sent back for further investigation. Reason: {reason}");
                    }
                }
                else
                {
                    await NotifyComplainantAsync(docket,
                        $"Docket #{docket.Id} is now {EDocketStatus.GetName(dto.NewStatusId)}." + (reason != null ? $" Note: {reason}" : ""));
                }

                return new ResultModel<Docket> { ResultId = EResultValue.Success, Message = "Docket status updated successfully.", Data = docket };
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while updating the docket: {ex.Message}");
            }
        }

        public async Task<ResultModel<Docket>> SetCasNumberAsync(long docketId, long actingUserId, DocketCasNumberDTO dto, string? ipAddress)
        {
            try
            {
                var (docket, _, error) = await LoadForStaffActionAsync(docketId, actingUserId, requireActive: true);
                if (error != null) return error;

                var casNumber = dto.CasNumber.Trim();
                if (await _context.Dockets.AnyAsync(d => d.CasNumber == casNumber && d.Id != docketId))
                {
                    return Fail($"CAS number {casNumber} is already assigned to another docket.");
                }

                var previous = docket!.CasNumber;
                docket.CasNumber = casNumber;
                docket.LastUpdated = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                await WriteAuditAsync(docket.Id, actingUserId, previous == null ? "CasNumberAssigned" : "CasNumberChanged", ipAddress);

                await NotifyComplainantAsync(docket, $"Your case (docket #{docket.Id}) has been registered under CAS number {casNumber}.");

                return new ResultModel<Docket> { ResultId = EResultValue.Success, Message = "CAS number saved.", Data = docket };
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while saving the CAS number: {ex.Message}");
            }
        }

        public async Task<ResultModel<Docket>> AssignInvestigatingOfficerAsync(long docketId, long commanderId, DocketAssignDTO dto, string? ipAddress)
        {
            try
            {
                var (docket, actingUser, error) = await LoadForStaffActionAsync(docketId, commanderId, requireActive: true);
                if (error != null) return error;

                if (actingUser!.RoleId != ERole.StationCommander)
                {
                    return Fail("Only the station commander can assign the investigating officer.");
                }

                var officer = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.OfficerId);
                if (officer == null || officer.RoleId != ERole.SapsOfficer || !officer.IsActive || officer.StationId != docket!.StationId)
                {
                    return Fail("The selected officer must be an active officer at this station.");
                }

                docket.InvestigatingOfficerId = officer.Id;
                docket.LastUpdated = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                await WriteAuditAsync(docket.Id, commanderId, "InvestigatingOfficerAssigned", ipAddress);

                await _notificationLogic.CreateAsync(officer.Id, docket.Id, $"You have been assigned as investigating officer on docket #{docket.Id}.");

                return new ResultModel<Docket> { ResultId = EResultValue.Success, Message = $"{officer.FullName} is now the investigating officer.", Data = docket };
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while assigning the officer: {ex.Message}");
            }
        }

        public async Task<ResultModel<Docket>> RequestClosureAsync(long docketId, long actingUserId, DocketClosureDTO dto, string? ipAddress)
        {
            try
            {
                var (docket, actingUser, error) = await LoadForStaffActionAsync(docketId, actingUserId);
                if (error != null) return error;

                if (docket!.StatusId is not (EDocketStatus.Open or EDocketStatus.UnderInvestigation))
                {
                    return Fail($"A docket that is {EDocketStatus.GetName(docket.StatusId)} cannot be submitted for closure.");
                }

                if (actingUser!.RoleId != ERole.StationCommander && docket.InvestigatingOfficerId != actingUserId)
                {
                    return Fail("Only the investigating officer or the station commander can submit this docket for closure.");
                }

                if (!EDocketClosureOutcome.All.Contains(dto.ClosureOutcomeId))
                {
                    return Fail("Please select a valid closure outcome.");
                }

                if (!await _context.DocketEvidence.AnyAsync(e => e.DocketId == docketId && e.ReviewStatusId == EEvidenceReviewStatus.Accepted))
                {
                    return Fail("At least one piece of accepted evidence is required before the docket can be closed.");
                }

                var isCommander = actingUser!.RoleId == ERole.StationCommander;
                var fromStatusId = docket.StatusId;
                var toStatusId = isCommander ? EDocketStatus.Closed : EDocketStatus.PendingClosure;
                var notes = dto.ClosureNotes.Trim();

                docket.ClosureOutcomeId = dto.ClosureOutcomeId;
                docket.ClosureNotes = notes;
                docket.StatusId = toStatusId;
                docket.LastUpdated = DateTime.UtcNow;
                if (isCommander)
                {
                    docket.ClosedDate = DateTime.UtcNow;
                    docket.ClosedByUserId = actingUserId;
                }
                docket.NavStatusHistory.Add(NewHistory(fromStatusId, toStatusId, actingUserId,
                    $"Outcome: {EDocketClosureOutcome.GetName(dto.ClosureOutcomeId)}. {notes}"));
                await _context.SaveChangesAsync();

                await WriteAuditAsync(docket.Id, actingUserId, isCommander ? "Closed" : "ClosureRequested", ipAddress);

                if (isCommander)
                {
                    await NotifyComplainantOfClosureAsync(docket);
                }
                else
                {
                    var commanderIds = await _context.Users
                        .Where(u => u.RoleId == ERole.StationCommander && u.StationId == docket.StationId && u.IsActive)
                        .Select(u => u.Id)
                        .ToListAsync();
                    foreach (var commanderId in commanderIds)
                    {
                        await _notificationLogic.CreateAsync(commanderId, docket.Id, $"Docket #{docket.Id} has been submitted for closure and needs your approval.");
                    }
                }

                return new ResultModel<Docket>
                {
                    ResultId = EResultValue.Success,
                    Message = isCommander ? "Docket closed." : "Docket submitted to the station commander for closure approval.",
                    Data = docket
                };
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while closing the docket: {ex.Message}");
            }
        }

        public async Task<ResultModel<Docket>> ApproveClosureAsync(long docketId, long commanderId, string? ipAddress)
        {
            try
            {
                var (docket, actingUser, error) = await LoadForStaffActionAsync(docketId, commanderId);
                if (error != null) return error;

                if (actingUser!.RoleId != ERole.StationCommander)
                {
                    return Fail("Only the station commander can approve a closure.");
                }

                if (docket!.StatusId != EDocketStatus.PendingClosure)
                {
                    return Fail("This docket is not awaiting closure approval.");
                }

                docket.StatusId = EDocketStatus.Closed;
                docket.ClosedDate = DateTime.UtcNow;
                docket.ClosedByUserId = commanderId;
                docket.LastUpdated = DateTime.UtcNow;
                docket.NavStatusHistory.Add(NewHistory(EDocketStatus.PendingClosure, EDocketStatus.Closed, commanderId, "Closure approved by station commander."));
                await _context.SaveChangesAsync();

                await WriteAuditAsync(docket.Id, commanderId, "ClosureApproved", ipAddress);

                await NotifyComplainantOfClosureAsync(docket);
                if (docket.InvestigatingOfficerId != null)
                {
                    await _notificationLogic.CreateAsync(docket.InvestigatingOfficerId.Value, docket.Id, $"The closure of docket #{docket.Id} has been approved.");
                }

                return new ResultModel<Docket> { ResultId = EResultValue.Success, Message = "Closure approved — docket closed.", Data = docket };
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while approving the closure: {ex.Message}");
            }
        }

        private async Task<(Docket? Docket, User? ActingUser, ResultModel<Docket>? Error)> LoadForStaffActionAsync(long docketId, long actingUserId, bool requireActive = false)
        {
            var actingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == actingUserId);
            var docket = await _context.Dockets
                .Include(d => d.NavReport)
                .FirstOrDefaultAsync(d => d.Id == docketId);

            if (docket == null)
            {
                return (null, null, Fail("Docket not found."));
            }

            if (actingUser == null || actingUser.StationId != docket.StationId)
            {
                return (null, null, Fail("You may only update dockets held at your own station."));
            }

            if (requireActive && EDocketStatus.IsFinal(docket.StatusId))
            {
                return (null, null, Fail($"This docket is {EDocketStatus.GetName(docket.StatusId)} and can no longer be changed."));
            }

            return (docket, actingUser, null);
        }

        private async Task NotifyComplainantOfClosureAsync(Docket docket)
        {
            await NotifyComplainantAsync(docket,
                $"Docket #{docket.Id} has been closed. Outcome: {EDocketClosureOutcome.GetName(docket.ClosureOutcomeId ?? 0)}.");
        }

        private async Task NotifyComplainantAsync(Docket docket, string message)
        {
            if (docket.NavReport != null)
            {
                await _notificationLogic.CreateAsync(docket.NavReport.ComplainantId, docket.Id, message);
            }
        }

        private Task WriteAuditAsync(long docketId, long userId, string actionType, string? ipAddress)
        {
            return _auditLogWriter.WriteAsync(new AuditLog
            {
                DocketId = docketId,
                UserId = userId,
                ActionType = actionType,
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress
            });
        }

        internal static DocketStatusHistory NewHistory(long? fromStatusId, long toStatusId, long changedByUserId, string? reason) => new()
        {
            FromStatusId = fromStatusId,
            ToStatusId = toStatusId,
            ChangedByUserId = changedByUserId,
            Reason = reason,
            ChangedDate = DateTime.UtcNow
        };

        private static ResultModel<Docket> Fail(string message) =>
            new() { ResultId = EResultValue.Fail, Message = message };
    }
}
