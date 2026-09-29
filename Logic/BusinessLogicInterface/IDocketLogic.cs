using Database.Entities;
using Logic.DTO;
using Logic.Model;

namespace Logic.BusinessLogicInterface
{
    public interface IDocketLogic
    {
        Task<ResultModel<Docket>> ConvertAsync(long officerId, DocketConvertDTO dto, string? ipAddress);
        Task<Docket?> GetByIdAsync(long id, long viewingUserId, string? ipAddress, bool includeDetails = false);
        Task<IEnumerable<Docket>> GetForComplainantAsync(long complainantId);
        Task<IEnumerable<Docket>> GetForStationAsync(long stationId);
        Task<PagedResult<Docket>> GetForComplainantPagedAsync(long complainantId, int page, int pageSize, long? statusId = null, CancellationToken ct = default);
        Task<PagedResult<Docket>> GetForStationPagedAsync(long stationId, int page, int pageSize, long? statusId = null, CancellationToken ct = default);
        Task<PagedResult<AssignedDocketItem>> GetAssignedPagedAsync(long officerId, int page, int pageSize, bool includeFinished, CancellationToken ct = default);
        Task<StationWorkQueue> GetStationWorkQueueAsync(long stationId, int overdueDays, CancellationToken ct = default);
        Task<IEnumerable<DocketStatusHistory>> GetStatusHistoryAsync(long docketId);
        Task<IEnumerable<User>> GetAssignableOfficersAsync(long stationId);
        Task<ResultModel<Docket>> UpdateStatusAsync(long docketId, long officerId, DocketStatusUpdateDTO dto, string? ipAddress);
        Task<ResultModel<Docket>> SetCasNumberAsync(long docketId, long actingUserId, DocketCasNumberDTO dto, string? ipAddress);
        Task<ResultModel<Docket>> AssignInvestigatingOfficerAsync(long docketId, long commanderId, DocketAssignDTO dto, string? ipAddress);
        Task<ResultModel<Docket>> RequestClosureAsync(long docketId, long actingUserId, DocketClosureDTO dto, string? ipAddress);
        Task<ResultModel<Docket>> ApproveClosureAsync(long docketId, long commanderId, string? ipAddress);
    }
}
