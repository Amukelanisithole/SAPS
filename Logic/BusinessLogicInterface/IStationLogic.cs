using Database.Entities;
using Logic.DTO;
using Logic.Model;

namespace Logic.BusinessLogicInterface
{
    public interface IStationLogic
    {
        Task<ResultModel<Station>> CreateAsync(StationDTO dto);
        Task<IEnumerable<Station>> GetAllAsync();
        Task<PagedResult<Station>> GetAllPagedAsync(int page, int pageSize, string? search = null, CancellationToken ct = default);
        Task<Station?> GetByIdAsync(long id);
    }
}
