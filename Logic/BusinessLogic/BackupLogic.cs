using Database.AppDBContext;
using Database.Entities;
using Logic.BusinessLogicInterface;
using Logic.Model;
using Microsoft.EntityFrameworkCore;
using static Logic.Enums.Enums;

namespace Logic.BusinessLogic
{
    public class BackupLogic : IBackupLogic
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditLogWriter _auditLogWriter;

        public BackupLogic(ApplicationDbContext context, IAuditLogWriter auditLogWriter)
        {
            _context = context;
            _auditLogWriter = auditLogWriter;
        }

        // The student prototype has no real backup infrastructure to trigger, so this
        // records the evidence FR-10/NFR-08 require (who ran it, when, whether restoration
        // was later verified) rather than performing a literal database backup.
        public async Task<ResultModel<BackupRecord>> PerformBackupAsync(long adminId)
        {
            try
            {
                var backup = new BackupRecord
                {
                    BackupDate = DateTime.UtcNow,
                    StatusId = EBackupStatus.Success,
                    PerformedBy = adminId,
                    RestorationTested = false
                };

                _context.BackupRecords.Add(backup);
                await _context.SaveChangesAsync();

                await _auditLogWriter.WriteAsync(new AuditLog
                {
                    UserId = adminId,
                    ActionType = $"BackupPerformed:{backup.Id}",
                    Timestamp = DateTime.UtcNow
                });

                return new ResultModel<BackupRecord> { ResultId = EResultValue.Success, Message = "Backup recorded successfully.", Data = backup };
            }
            catch (Exception ex)
            {
                return new ResultModel<BackupRecord> { ResultId = EResultValue.Fail, Message = $"An error occurred while recording the backup: {ex.Message}" };
            }
        }

        public async Task<IEnumerable<BackupRecord>> GetAllAsync()
        {
            return await _context.BackupRecords.OrderByDescending(b => b.BackupDate).ToListAsync();
        }

        public async Task<ResultModel<BackupRecord>> MarkRestorationTestedAsync(long backupId, long adminId)
        {
            try
            {
                var backup = await _context.BackupRecords.FirstOrDefaultAsync(b => b.Id == backupId);
                if (backup == null)
                {
                    return new ResultModel<BackupRecord> { ResultId = EResultValue.Fail, Message = "Backup record not found." };
                }

                backup.RestorationTested = true;
                await _context.SaveChangesAsync();

                await _auditLogWriter.WriteAsync(new AuditLog
                {
                    UserId = adminId,
                    ActionType = $"RestorationTested:{backup.Id}",
                    Timestamp = DateTime.UtcNow
                });

                return new ResultModel<BackupRecord> { ResultId = EResultValue.Success, Message = "Restoration test recorded successfully.", Data = backup };
            }
            catch (Exception ex)
            {
                return new ResultModel<BackupRecord> { ResultId = EResultValue.Fail, Message = $"An error occurred while recording the restoration test: {ex.Message}" };
            }
        }
    }
}
