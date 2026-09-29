using Database.Entities;
using Logic.Model;

namespace Logic.BusinessLogicInterface
{
    public interface INotificationLogic
    {
        Task<Notification> CreateAsync(long userId, long? docketId, string message);
        Task<IEnumerable<Notification>> GetMineAsync(long userId);
        Task<PagedResult<Notification>> GetMinePagedAsync(long userId, int page, int pageSize, bool? readStatus = null, CancellationToken ct = default);
        Task<Notification?> GetMineByIdAsync(long notificationId, long userId);
        Task<ResultModel> MarkAsReadAsync(long notificationId, long userId);
    }
}
