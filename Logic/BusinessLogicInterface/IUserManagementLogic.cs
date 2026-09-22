using Database.Entities;
using Logic.DTO;
using Logic.Model;

namespace Logic.BusinessLogicInterface
{
    public interface IUserManagementLogic
    {
        Task<ResultModel<User>> CreateUserAsync(long performedByAdminId, CreateUserDTO dto);
        Task<IEnumerable<User>> GetAllAsync();
        Task<PagedResult<User>> GetAllPagedAsync(int page, int pageSize, long? roleId = null, long? stationId = null, bool? isActive = null, CancellationToken ct = default);
        Task<User?> GetByIdAsync(long id);
        Task<ResultModel<User>> UpdateUserAsync(long performedByAdminId, long targetUserId, UpdateUserDTO dto);
        Task<ResultModel<User>> SetActiveStatusAsync(long performedByAdminId, long targetUserId, bool isActive);
        Task<ResultModel<User>> UpdateOwnProfileAsync(long userId, UpdateProfileDTO dto);
        Task<ResultModel> ChangePasswordAsync(long userId, ChangePasswordDTO dto);
    }
}
