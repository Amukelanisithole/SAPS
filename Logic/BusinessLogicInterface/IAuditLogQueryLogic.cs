using Logic.DTO;
using Logic.Model;

namespace Logic.BusinessLogicInterface
{
    public interface IAuditLogQueryLogic
    {
        Task<PagedResult<AuditLogViewDTO>> GetPagedAsync(int page, int pageSize, string? search = null, CancellationToken ct = default);
    }
}
