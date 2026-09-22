using Database.Entities;
using Logic.DTO;
using Logic.Model;

namespace Logic.BusinessLogicInterface
{
    public interface IDocketLogic
    {
        Task<ResultModel<Docket>> ConvertAsync(long officerId, DocketConvertDTO dto, string? ipAddress);
        Task<Docket?> GetByIdAsync(long id, long viewingUserId, string? ipAddress);
        Task<IEnumerable<Docket>> GetForComplainantAsync(long complainantId);
        Task<IEnumerable<Docket>> GetForStationAsync(long stationId);
        Task<PagedResult<Docket>> GetForComplainantPagedAsync(long complainantId, int page, int pageSize, long? statusId = null, CancellationToken ct = default);
        Task<PagedResult<Docket>> GetForStationPagedAsync(long stationId, int page, int pageSize, long? statusId = null, CancellationToken ct = default);
        Task<ResultModel<Docket>> UpdateStatusAsync(long docketId, long officerId, DocketStatusUpdateDTO dto, string? ipAddress);
    }
}
