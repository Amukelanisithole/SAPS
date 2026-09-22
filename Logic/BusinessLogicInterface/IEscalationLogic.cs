using Database.Entities;
using Logic.DTO;
using Logic.Model;

namespace Logic.BusinessLogicInterface
{
    public interface IEscalationLogic
    {
        Task<ResultModel<Escalation>> CreateAsync(long complainantId, EscalationDTO dto);
        Task<IEnumerable<Escalation>> GetMineAsync(long complainantId);
        Task<IEnumerable<Escalation>> GetForStationAsync(long stationId);
        Task<PagedResult<Escalation>> GetMinePagedAsync(long complainantId, int page, int pageSize, long? statusId = null, CancellationToken ct = default);
        Task<PagedResult<Escalation>> GetForStationPagedAsync(long stationId, int page, int pageSize, long? statusId = null, CancellationToken ct = default);
        Task<IEnumerable<Escalation>> GetByDocketIdAsync(long docketId);
        Task<ResultModel<Escalation>> UpdateStatusAsync(long escalationId, long commanderId, EscalationStatusUpdateDTO dto);
    }
}
