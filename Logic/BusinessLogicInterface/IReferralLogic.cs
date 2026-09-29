using Database.Entities;
using Logic.DTO;
using Logic.Model;

namespace Logic.BusinessLogicInterface
{
    public interface IReferralLogic
    {
        Task<ResultModel<Referral>> CreateAsync(long officerId, ReferralDTO dto, string? ipAddress);
        Task<ResultModel<Referral>> AcceptAsync(long referralId, long commanderId, string? ipAddress);
        Task<ResultModel<Referral>> DeclineAsync(long referralId, long commanderId, string reason, string? ipAddress);
        Task<ResultModel<Referral>> CancelAsync(long referralId, long userId, string? ipAddress);
        Task<IEnumerable<Referral>> GetByDocketIdAsync(long docketId);
        Task<Referral?> GetPendingForDocketAsync(long docketId);
        Task<IEnumerable<Referral>> GetPendingIncomingAsync(long stationId);
    }
}
