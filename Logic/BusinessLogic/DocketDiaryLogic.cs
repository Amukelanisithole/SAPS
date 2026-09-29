using Database.AppDBContext;
using Database.Entities;
using Logic.BusinessLogicInterface;
using Logic.DTO;
using Logic.Model;
using Microsoft.EntityFrameworkCore;
using static Logic.Enums.Enums;

namespace Logic.BusinessLogic
{
    public class DocketDiaryLogic : IDocketDiaryLogic
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationLogic _notificationLogic;
        private readonly IAuditLogWriter _auditLogWriter;

        public DocketDiaryLogic(ApplicationDbContext context, INotificationLogic notificationLogic, IAuditLogWriter auditLogWriter)
        {
            _context = context;
            _notificationLogic = notificationLogic;
            _auditLogWriter = auditLogWriter;
        }

        public async Task<ResultModel> AddAsync(long docketId, long authorId, DocketDiaryEntryDTO dto, string? ipAddress)
        {
            try
            {
                var docket = await _context.Dockets
                    .Include(d => d.NavReport)
                    .FirstOrDefaultAsync(d => d.Id == docketId);
                if (docket == null)
                {
                    return Fail("Docket not found.");
                }

                var author = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == authorId);
                if (author == null || author.RoleId is not (ERole.SapsOfficer or ERole.StationCommander) || author.StationId != docket.StationId)
                {
                    return Fail("Only staff at the docket's station can write in its investigation diary.");
                }

                if (EDocketStatus.IsFinal(docket.StatusId))
                {
                    return Fail($"This docket is {EDocketStatus.GetName(docket.StatusId)} — the diary is closed.");
                }

                if (!EDiaryEntryType.All.Contains(dto.EntryTypeId))
                {
                    return Fail("Please choose a valid action type.");
                }

                if (string.IsNullOrWhiteSpace(dto.Notes))
                {
                    return Fail("Please describe what was done.");
                }

                var actionDateUtc = dto.ActionDate.Kind == DateTimeKind.Utc ? dto.ActionDate : dto.ActionDate.ToUniversalTime();
                if (actionDateUtc > DateTime.UtcNow.AddMinutes(5))
                {
                    return Fail("The action date cannot be in the future.");
                }

                var entry = new DocketDiaryEntry
                {
                    DocketId = docketId,
                    AuthorId = authorId,
                    EntryTypeId = dto.EntryTypeId,
                    ActionDate = actionDateUtc,
                    Notes = dto.Notes.Trim(),
                    SharedWithComplainant = dto.SharedWithComplainant,
                    CreatedDate = DateTime.UtcNow
                };

                _context.DocketDiaryEntries.Add(entry);
                docket.LastUpdated = DateTime.UtcNow;
                docket.LastOverdueAlertDate = null;
                await _context.SaveChangesAsync();

                await _auditLogWriter.WriteAsync(new AuditLog
                {
                    DocketId = docketId,
                    UserId = authorId,
                    ActionType = $"DiaryEntry: {EDiaryEntryType.GetName(dto.EntryTypeId)}",
                    Timestamp = DateTime.UtcNow,
                    IpAddress = ipAddress
                });

                if (entry.SharedWithComplainant && docket.NavReport != null)
                {
                    await _notificationLogic.CreateAsync(docket.NavReport.ComplainantId, docketId,
                        $"Progress on docket #{docketId} — {EDiaryEntryType.GetName(entry.EntryTypeId)}: {Truncate(entry.Notes, 200)}");
                }

                return new ResultModel
                {
                    ResultId = EResultValue.Success,
                    Message = entry.SharedWithComplainant ? "Diary entry added and shared with the complainant." : "Diary entry added."
                };
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while adding the diary entry: {ex.Message}");
            }
        }

        public async Task<IEnumerable<DocketDiaryEntry>> GetByDocketIdAsync(long docketId, bool sharedOnly)
        {
            var query = _context.DocketDiaryEntries
                .Include(e => e.NavAuthor)
                .Where(e => e.DocketId == docketId);

            if (sharedOnly)
            {
                query = query.Where(e => e.SharedWithComplainant);
            }

            return await query
                .OrderByDescending(e => e.ActionDate)
                .ThenByDescending(e => e.Id)
                .ToListAsync();
        }

        public async Task<DateTime> GetLastActivityAsync(Docket docket)
        {
            var lastEntry = await _context.DocketDiaryEntries
                .Where(e => e.DocketId == docket.Id)
                .MaxAsync(e => (DateTime?)e.CreatedDate);
            return lastEntry ?? docket.OpenedDate;
        }

        private static string Truncate(string value, int max) =>
            value.Length <= max ? value : value[..max].TrimEnd() + "…";

        private static ResultModel Fail(string message) =>
            new() { ResultId = EResultValue.Fail, Message = message };
    }
}
