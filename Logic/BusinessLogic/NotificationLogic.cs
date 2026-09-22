using Database.AppDBContext;
using Database.Entities;
using Logic.BusinessLogicInterface;
using Logic.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static Logic.Enums.Enums;

namespace Logic.BusinessLogic
{
    public class NotificationLogic : INotificationLogic
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly ISmsSender _smsSender;
        private readonly AppSettings _appSettings;
        private readonly ILogger<NotificationLogic> _logger;

        public NotificationLogic(
            ApplicationDbContext context,
            IEmailTemplateService emailTemplateService,
            ISmsSender smsSender,
            IOptions<AppSettings> appSettings,
            ILogger<NotificationLogic> logger)
        {
            _context = context;
            _emailTemplateService = emailTemplateService;
            _smsSender = smsSender;
            _appSettings = appSettings.Value;
            _logger = logger;
        }

        public async Task<Notification> CreateAsync(long userId, long? docketId, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                DocketId = docketId,
                Message = message,
                SentDate = DateTime.UtcNow,
                ReadStatus = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Every notification is also written to the database row above
            // regardless of what happens here, so a user always sees it next
            // time they log in even if delivery fails or isn't configured —
            // this is best-effort, not the source of truth.
            await TryDeliverAsync(userId, docketId, message);

            return notification;
        }

        private async Task TryDeliverAsync(long userId, long? docketId, string message)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return;

            try
            {
                if (!string.IsNullOrWhiteSpace(user.Email))
                {
                    var baseUrl = _appSettings.BaseUrl?.TrimEnd('/') ?? string.Empty;
                    var caseLink = docketId.HasValue && !string.IsNullOrEmpty(baseUrl)
                        ? $"{baseUrl}/Dockets/Details/{docketId}"
                        : string.Empty;

                    await _emailTemplateService.SendAsync(EEmailTemplates.CaseNotification, user.Email, new Dictionary<string, string>
                    {
                        ["FullName"] = user.FullName,
                        ["Message"] = message,
                        ["CaseLink"] = caseLink,
                        ["LogoUrl"] = string.IsNullOrEmpty(baseUrl) ? string.Empty : $"{baseUrl}/branding/saps-logo.png"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to email notification to user {UserId}.", userId);
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
                {
                    await _smsSender.SendAsync(user.PhoneNumber, message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to SMS notification to user {UserId}.", userId);
            }
        }

        public async Task<IEnumerable<Notification>> GetMineAsync(long userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.SentDate)
                .ToListAsync();
        }

        public async Task<PagedResult<Notification>> GetMinePagedAsync(long userId, int page, int pageSize, bool? readStatus = null, CancellationToken ct = default)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var query = _context.Notifications.Where(n => n.UserId == userId);
            if (readStatus != null)
            {
                query = query.Where(n => n.ReadStatus == readStatus);
            }

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(n => n.SentDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResult<Notification> { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount };
        }

        public async Task<ResultModel> MarkAsReadAsync(long notificationId, long userId)
        {
            var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId);
            if (notification == null)
            {
                return new ResultModel { ResultId = EResultValue.Fail, Message = "Notification not found." };
            }

            if (notification.UserId != userId)
            {
                return new ResultModel { ResultId = EResultValue.Fail, Message = "You may only mark your own notifications as read." };
            }

            notification.ReadStatus = true;
            await _context.SaveChangesAsync();

            return new ResultModel { ResultId = EResultValue.Success, Message = "Notification marked as read." };
        }
    }
}
