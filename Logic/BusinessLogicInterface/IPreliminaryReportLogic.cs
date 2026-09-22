using Database.Entities;
using Logic.DTO;
using Logic.Model;

namespace Logic.BusinessLogicInterface
{
    public interface IPreliminaryReportLogic
    {
        Task<ResultModel<PreliminaryReport>> SubmitAsync(long complainantId, PreliminaryReportDTO dto);
        Task<PreliminaryReport?> GetByIdAsync(long id);
        Task<IEnumerable<PreliminaryReport>> GetMineAsync(long complainantId);
        Task<IEnumerable<PreliminaryReport>> GetForReviewAsync(long stationId);
        Task<PagedResult<PreliminaryReport>> GetMinePagedAsync(long complainantId, int page, int pageSize, long? statusId = null, CancellationToken ct = default);
        Task<PagedResult<PreliminaryReport>> GetForReviewPagedAsync(long stationId, int page, int pageSize, long? statusId = null, CancellationToken ct = default);
    }
}
