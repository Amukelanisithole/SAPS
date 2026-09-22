using Database.Entities;
using Logic.DTO;
using Logic.Model;

namespace Logic.BusinessLogicInterface
{
    public interface IReferralLogic
    {
        Task<ResultModel<Referral>> CreateAsync(long officerId, ReferralDTO dto, string? ipAddress);
        Task<IEnumerable<Referral>> GetByDocketIdAsync(long docketId);
    }
}
