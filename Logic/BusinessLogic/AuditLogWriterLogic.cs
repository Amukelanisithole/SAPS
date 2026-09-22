using Database.AppDBContext;
using Database.Entities;
using Logic.BusinessLogicInterface;

namespace Logic.BusinessLogic
{
    public class AuditLogWriterLogic : IAuditLogWriter
    {
        private readonly ApplicationDbContext _context;

        public AuditLogWriterLogic(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AuditLog> WriteAsync(AuditLog log, CancellationToken ct = default)
        {
            if (log.Timestamp == default) log.Timestamp = DateTime.UtcNow;

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync(ct);

            return log;
        }
    }
}
