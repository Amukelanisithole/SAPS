using Database.AppDBContext;
using Logic.BusinessLogicInterface;
using Logic.DTO;
using Logic.Model;
using Microsoft.EntityFrameworkCore;

namespace Logic.BusinessLogic
{
    public class AuditLogQueryLogic : IAuditLogQueryLogic
    {
        private readonly ApplicationDbContext _context;

        public AuditLogQueryLogic(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<AuditLogViewDTO>> GetPagedAsync(int page, int pageSize, string? search = null, CancellationToken ct = default)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var query = _context.AuditLogs.Include(a => a.NavUser).Include(a => a.NavAffidavit).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(a =>
                    a.ActionType.Contains(term) ||
                    (a.NavUser != null && a.NavUser.Email.Contains(term)) ||
                    (a.NavUser != null && a.NavUser.FullName.Contains(term)));
            }

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AuditLogViewDTO
                {
                    Id = a.Id,
                    Timestamp = a.Timestamp,
                    ActionType = a.ActionType,
                    UserId = a.UserId,
                    UserFullName = a.NavUser != null ? a.NavUser.FullName : "Unknown",
                    UserEmail = a.NavUser != null ? a.NavUser.Email : "",
                    DocketId = a.DocketId,
                    AffidavitId = a.AffidavitId,
                    AffidavitReferenceNumber = a.NavAffidavit != null ? a.NavAffidavit.ReferenceNumber : null,
                    EmergencyAlertId = a.EmergencyAlertId,
                    IpAddress = a.IpAddress
                })
                .ToListAsync(ct);

            return new PagedResult<AuditLogViewDTO>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
    }
}
