using Database.Entities;
using Logic.DTO;
using Logic.Model;

namespace Logic.BusinessLogicInterface
{
    public interface IDocketEvidenceLogic
    {
        Task<ResultModel> AddAsync(long docketId, long uploadedByUserId, DocketEvidenceDTO dto, string? ipAddress);
        Task<ResultModel> ReviewAsync(long evidenceId, long reviewerId, bool accept, string? note, string? ipAddress);
        Task<int> CountPendingForInvestigatorAsync(long officerId);
        Task<IEnumerable<DocketEvidence>> GetByDocketIdAsync(long docketId);
        Task<(DocketEvidence Evidence, byte[] Bytes)?> GetFileAsync(long evidenceId);
    }
}
