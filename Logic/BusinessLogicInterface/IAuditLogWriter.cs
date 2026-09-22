using Database.Entities;

namespace Logic.BusinessLogicInterface
{
    public interface IAuditLogWriter
    {
        Task<AuditLog> WriteAsync(AuditLog log, CancellationToken ct = default);
    }
}
