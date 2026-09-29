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

        public async Task<ResultModel> AddAsync(long authorId, DocketCommentDTO dto)
        {
            var docket = await _context.Dockets
                .Include(d => d.NavReport)
                .FirstOrDefaultAsync(d => d.Id == dto.DocketId);

            if (docket == null)
            {
                return new ResultModel { ResultId = EResultValue.Fail, Message = "Docket not found." };
            }

            if (EDocketStatus.IsFinal(docket.StatusId))
            {
                return new ResultModel { ResultId = EResultValue.Fail, Message = $"This docket is {EDocketStatus.GetName(docket.StatusId)} — messaging is closed." };
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
            var recipientId = authorId == complainantId ? docket.InvestigatingOfficerId ?? docket.ReviewingOfficerId : complainantId;

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
