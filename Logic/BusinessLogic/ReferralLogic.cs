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
                    return new ResultModel<Referral> { ResultId = EResultValue.Fail, Message = "Docket not found." };
                }

                // NFR-02: only an officer holding the docket at their own station can move it onward.
                if (officer?.StationId != docket.StationId)
                {
                    return new ResultModel<Referral> { ResultId = EResultValue.Fail, Message = "You may only refer dockets held at your own station." };
                }

                if (docket.StationId == dto.ToStationId)
                {
                    return new ResultModel<Referral> { ResultId = EResultValue.Fail, Message = "Cannot refer a docket to the station it is already at." };
                }

                var destinationExists = await _context.Stations.AnyAsync(s => s.Id == dto.ToStationId);
                if (!destinationExists)
                {
                    return new ResultModel<Referral> { ResultId = EResultValue.Fail, Message = "Destination station not found." };
                }

                var referral = new Referral
                {
                    DocketId = docket.Id,
                    FromStationId = docket.StationId,
                    ToStationId = dto.ToStationId,
                    ReferredByOfficerId = officerId,
                    ReferralDate = DateTime.UtcNow,
                    Reason = dto.Reason
                };

                _context.Referrals.Add(referral);

                docket.StationId = dto.ToStationId;
                docket.StatusId = EDocketStatus.Referred;
                docket.LastUpdated = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                await _auditLogWriter.WriteAsync(new AuditLog
                {
                    DocketId = docket.Id,
                    UserId = officerId,
                    ActionType = "Referred",
                    Timestamp = DateTime.UtcNow,
                    IpAddress = ipAddress
                });

                if (docket.NavReport != null)
                {
                    await _notificationLogic.CreateAsync(
                        docket.NavReport.ComplainantId,
                        docket.Id,
                        $"Your case (docket #{docket.Id}) has been referred to another station.");
                }

                return new ResultModel<Referral> { ResultId = EResultValue.Success, Message = "Docket referred successfully.", Data = referral };
            }
            catch (Exception ex)
            {
                return new ResultModel<Referral> { ResultId = EResultValue.Fail, Message = $"An error occurred while referring the docket: {ex.Message}" };
            }
        }

        public async Task<IEnumerable<Referral>> GetByDocketIdAsync(long docketId)
        {
            return await _context.Referrals
                .Where(r => r.DocketId == docketId)
                .OrderByDescending(r => r.ReferralDate)
                .ToListAsync();
        }
    }
}
