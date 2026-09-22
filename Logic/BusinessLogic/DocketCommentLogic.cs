using Database.AppDBContext;
using Database.Entities;
using Logic.BusinessLogicInterface;
using Logic.DTO;
using Logic.Model;
using Microsoft.EntityFrameworkCore;
using static Logic.Enums.Enums;

namespace Logic.BusinessLogic
{
    public class DocketCommentLogic : IDocketCommentLogic
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationLogic _notificationLogic;

        public DocketCommentLogic(ApplicationDbContext context, INotificationLogic notificationLogic)
        {
            _context = context;
            _notificationLogic = notificationLogic;
        }

        public async Task<IEnumerable<DocketCommentViewDTO>> GetByDocketIdAsync(long docketId)
        {
            return await _context.DocketComments
                .Include(c => c.NavAuthor)
                .Where(c => c.DocketId == docketId)
                .OrderBy(c => c.CreatedDate)
                .Select(c => new DocketCommentViewDTO
                {
                    Id = c.Id,
                    AuthorId = c.AuthorId,
                    AuthorFullName = c.NavAuthor != null ? c.NavAuthor.FullName : "Unknown",
                    AuthorRoleName = c.NavAuthor != null ? ERole.GetName(c.NavAuthor.RoleId) : "Unknown",
                    Message = c.Message,
                    CreatedDate = c.CreatedDate
                })
                .ToListAsync();
        }

        // Authorization (is this author actually allowed on this docket?) is
        // enforced by the controller, same as every other Dockets action —
        // this just persists the message and notifies whichever side didn't
        // write it, so a two-way conversation doesn't require polling.
        public async Task<ResultModel> AddAsync(long authorId, DocketCommentDTO dto)
        {
            var docket = await _context.Dockets
                .Include(d => d.NavReport)
                .FirstOrDefaultAsync(d => d.Id == dto.DocketId);

            if (docket == null)
            {
                return new ResultModel { ResultId = EResultValue.Fail, Message = "Docket not found." };
            }

            _context.DocketComments.Add(new DocketComment
            {
                DocketId = dto.DocketId,
                AuthorId = authorId,
                Message = dto.Message,
                CreatedDate = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            var complainantId = docket.NavReport?.ComplainantId;
            var recipientId = authorId == complainantId ? docket.ReviewingOfficerId : complainantId;

            if (recipientId.HasValue && recipientId.Value != authorId)
            {
                await _notificationLogic.CreateAsync(
                    recipientId.Value,
                    docket.Id,
                    $"New message on docket #{docket.Id}.");
            }

            return new ResultModel { ResultId = EResultValue.Success, Message = "Message sent." };
        }
    }
}
