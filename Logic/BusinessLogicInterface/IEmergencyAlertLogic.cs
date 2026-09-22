using Database.Entities;
using Logic.DTO;
using Logic.Model;

namespace Logic.BusinessLogicInterface
{
    public interface IEmergencyAlertLogic
    {
        Task<ResultModel<EmergencyAlert>> SubmitAsync(long complainantId, EmergencyAlertDTO dto, string? ipAddress = null);
        Task<EmergencyAlert?> GetByIdAsync(long id);
        Task<IEnumerable<EmergencyAlert>> GetAllAsync();
        Task<IEnumerable<EmergencyAlert>> GetForStationAsync(long stationId);
        Task<PagedResult<EmergencyAlert>> GetAllPagedAsync(int page, int pageSize, long? statusId = null, long? stationId = null, CancellationToken ct = default);
        Task<PagedResult<EmergencyAlert>> GetForStationPagedAsync(long stationId, int page, int pageSize, CancellationToken ct = default);
        Task<ResultModel<EmergencyAlert>> AcknowledgeAsync(long officerId, long alertId, string? ipAddress = null);
    }
}
