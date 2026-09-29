using Database.AppDBContext;
using Database.Entities;
using Logic.BusinessLogicInterface;
using Logic.DTO;
using Logic.Model;
using Microsoft.EntityFrameworkCore;
using static Logic.Enums.Enums;

namespace Logic.BusinessLogic
{
    public class ReferralLogic : IReferralLogic
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditLogWriter _auditLogWriter;
        private readonly INotificationLogic _notificationLogic;

        public ReferralLogic(ApplicationDbContext context, IAuditLogWriter auditLogWriter, INotificationLogic notificationLogic)
        {
            _context = context;
            _auditLogWriter = auditLogWriter;
            _notificationLogic = notificationLogic;
        }

        public async Task<ResultModel<Referral>> CreateAsync(long officerId, ReferralDTO dto, string? ipAddress)
        {
            try
            {
                var officer = await _context.Users.FirstOrDefaultAsync(u => u.Id == officerId);

                var docket = await _context.Dockets
                    .Include(d => d.NavReport)
                    .FirstOrDefaultAsync(d => d.Id == dto.DocketId);
                if (docket == null)
                {
                    return Fail("Docket not found.");
                }

                if (officer?.StationId != docket.StationId)
                {
                    return Fail("You may only refer dockets held at your own station.");
                }

                if (docket.StatusId is not (EDocketStatus.Open or EDocketStatus.UnderInvestigation))
                {
                    return Fail($"A docket that is {EDocketStatus.GetName(docket.StatusId)} cannot be referred.");
                }

                if (docket.StationId == dto.ToStationId)
                {
                    return Fail("Cannot refer a docket to the station it is already at.");
                }

                var destination = await _context.Stations.FirstOrDefaultAsync(s => s.Id == dto.ToStationId);
                if (destination == null)
                {
                    return Fail("Destination station not found.");
                }

                var referral = new Referral
                {
                    DocketId = docket.Id,
                    FromStationId = docket.StationId,
                    ToStationId = dto.ToStationId,
                    ReferredByOfficerId = officerId,
                    ReferralDate = DateTime.UtcNow,
                    Reason = dto.Reason,
                    StatusId = EReferralStatus.Pending,
                    PreviousDocketStatusId = docket.StatusId
                };

                _context.Referrals.Add(referral);

                docket.NavStatusHistory.Add(DocketLogic.NewHistory(docket.StatusId, EDocketStatus.Referred, officerId,
                    $"Referral to {destination.StationName} requested: {dto.Reason}"));
                docket.StatusId = EDocketStatus.Referred;
                docket.LastUpdated = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                await WriteAuditAsync(docket.Id, officerId, "ReferralRequested", ipAddress);

                await NotifyCommandersAsync(dto.ToStationId, docket.Id,
                    $"Docket #{docket.Id} has been referred to your station and is waiting for you to accept or decline it.");

                return new ResultModel<Referral> { ResultId = EResultValue.Success, Message = $"Referral sent — {destination.StationName} must accept it before the docket moves.", Data = referral };
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while referring the docket: {ex.Message}");
            }
        }

        public async Task<ResultModel<Referral>> AcceptAsync(long referralId, long commanderId, string? ipAddress)
        {
            try
            {
                var (referral, error) = await LoadPendingAsync(referralId);
                if (error != null) return error;

                var commander = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == commanderId);
                if (commander == null || commander.RoleId != ERole.StationCommander || commander.StationId != referral!.ToStationId)
                {
                    return Fail("Only the receiving station's commander can accept this referral.");
                }

                var docket = referral.NavDocket!;
                referral.StatusId = EReferralStatus.Accepted;
                referral.RespondedByUserId = commanderId;
                referral.RespondedDate = DateTime.UtcNow;

                docket.NavStatusHistory.Add(DocketLogic.NewHistory(EDocketStatus.Referred, EDocketStatus.Open, commanderId,
                    $"Referral accepted by {referral.NavToStation?.StationName}."));
                docket.StationId = referral.ToStationId;
                docket.StatusId = EDocketStatus.Open;
                docket.InvestigatingOfficerId = null;
                docket.LastOverdueAlertDate = null;
                docket.LastUpdated = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                await WriteAuditAsync(docket.Id, commanderId, "ReferralAccepted", ipAddress);

                await _notificationLogic.CreateAsync(referral.ReferredByOfficerId, docket.Id,
                    $"{referral.NavToStation?.StationName} accepted the referral of docket #{docket.Id}.");
                if (docket.NavReport != null)
                {
                    await _notificationLogic.CreateAsync(docket.NavReport.ComplainantId, docket.Id,
                        $"Your case (docket #{docket.Id}) has been transferred to {referral.NavToStation?.StationName}, which will continue the investigation.");
                }

                return new ResultModel<Referral> { ResultId = EResultValue.Success, Message = "Referral accepted — the docket is now held at your station. Assign an investigating officer.", Data = referral };
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while accepting the referral: {ex.Message}");
            }
        }

        public async Task<ResultModel<Referral>> DeclineAsync(long referralId, long commanderId, string reason, string? ipAddress)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(reason))
                {
                    return Fail("Please give a reason for declining the referral.");
                }

                var (referral, error) = await LoadPendingAsync(referralId);
                if (error != null) return error;

                var commander = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == commanderId);
                if (commander == null || commander.RoleId != ERole.StationCommander || commander.StationId != referral!.ToStationId)
                {
                    return Fail("Only the receiving station's commander can decline this referral.");
                }

                Close(referral, EReferralStatus.Declined, commanderId, reason.Trim(),
                    $"Referral declined by {referral.NavToStation?.StationName}: {reason.Trim()}");
                await _context.SaveChangesAsync();

                await WriteAuditAsync(referral.DocketId, commanderId, "ReferralDeclined", ipAddress);

                await _notificationLogic.CreateAsync(referral.ReferredByOfficerId, referral.DocketId,
                    $"{referral.NavToStation?.StationName} declined the referral of docket #{referral.DocketId}. Reason: {reason.Trim()}");

                return new ResultModel<Referral> { ResultId = EResultValue.Success, Message = "Referral declined — the sending station has been notified.", Data = referral };
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while declining the referral: {ex.Message}");
            }
        }

        public async Task<ResultModel<Referral>> CancelAsync(long referralId, long userId, string? ipAddress)
        {
            try
            {
                var (referral, error) = await LoadPendingAsync(referralId);
                if (error != null) return error;

                var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null || user.RoleId is not (ERole.SapsOfficer or ERole.StationCommander) || user.StationId != referral!.FromStationId)
                {
                    return Fail("Only the sending station can cancel this referral.");
                }

                Close(referral, EReferralStatus.Cancelled, userId, null, "Referral cancelled by the sending station.");
                await _context.SaveChangesAsync();

                await WriteAuditAsync(referral.DocketId, userId, "ReferralCancelled", ipAddress);

                return new ResultModel<Referral> { ResultId = EResultValue.Success, Message = "Referral cancelled.", Data = referral };
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while cancelling the referral: {ex.Message}");
            }
        }

        public async Task<IEnumerable<Referral>> GetByDocketIdAsync(long docketId)
        {
            return await _context.Referrals
                .Include(r => r.NavFromStation)
                .Include(r => r.NavToStation)
                .Include(r => r.NavRespondedBy)
                .Where(r => r.DocketId == docketId)
                .OrderByDescending(r => r.ReferralDate)
                .ToListAsync();
        }

        public async Task<Referral?> GetPendingForDocketAsync(long docketId)
        {
            return await _context.Referrals
                .Include(r => r.NavFromStation)
                .Include(r => r.NavToStation)
                .FirstOrDefaultAsync(r => r.DocketId == docketId && r.StatusId == EReferralStatus.Pending);
        }

        public async Task<IEnumerable<Referral>> GetPendingIncomingAsync(long stationId)
        {
            return await _context.Referrals
                .Include(r => r.NavFromStation)
                .Include(r => r.NavDocket).ThenInclude(d => d!.NavReport)
                .Where(r => r.ToStationId == stationId && r.StatusId == EReferralStatus.Pending)
                .OrderBy(r => r.ReferralDate)
                .ToListAsync();
        }

        private async Task<(Referral? Referral, ResultModel<Referral>? Error)> LoadPendingAsync(long referralId)
        {
            var referral = await _context.Referrals
                .Include(r => r.NavToStation)
                .Include(r => r.NavDocket).ThenInclude(d => d!.NavReport)
                .FirstOrDefaultAsync(r => r.Id == referralId);

            if (referral?.NavDocket == null)
            {
                return (null, Fail("Referral not found."));
            }

            if (referral.StatusId != EReferralStatus.Pending)
            {
                return (null, Fail($"This referral has already been {EReferralStatus.GetName(referral.StatusId).ToLower()}."));
            }

            return (referral, null);
        }

        private static void Close(Referral referral, long statusId, long userId, string? note, string historyReason)
        {
            var docket = referral.NavDocket!;
            var restoredStatusId = referral.PreviousDocketStatusId ?? EDocketStatus.Open;

            referral.StatusId = statusId;
            referral.RespondedByUserId = userId;
            referral.RespondedDate = DateTime.UtcNow;
            referral.ResponseNote = note;

            docket.NavStatusHistory.Add(DocketLogic.NewHistory(EDocketStatus.Referred, restoredStatusId, userId, historyReason));
            docket.StatusId = restoredStatusId;
            docket.LastUpdated = DateTime.UtcNow;
        }

        private async Task NotifyCommandersAsync(long stationId, long docketId, string message)
        {
            var commanderIds = await _context.Users
                .Where(u => u.RoleId == ERole.StationCommander && u.StationId == stationId && u.IsActive)
                .Select(u => u.Id)
                .ToListAsync();
            foreach (var commanderId in commanderIds)
            {
                await _notificationLogic.CreateAsync(commanderId, docketId, message);
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

        private static ResultModel<Referral> Fail(string message) =>
            new() { ResultId = EResultValue.Fail, Message = message };
    }
}
