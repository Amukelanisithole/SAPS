using Database.Entities;
using Logic.Model;

namespace Logic.BusinessLogicInterface
{
    public interface IBackupLogic
    {
        Task<ResultModel<BackupRecord>> PerformBackupAsync(long adminId);
        Task<IEnumerable<BackupRecord>> GetAllAsync();
        Task<ResultModel<BackupRecord>> MarkRestorationTestedAsync(long backupId, long adminId);
    }
}
