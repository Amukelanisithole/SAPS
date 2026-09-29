using Database.AppDBContext;
using Logic.BusinessLogicInterface;
using Logic.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static Logic.Enums.Enums;

namespace Logic.BusinessLogic
{
    public class DocketFollowUpLogic : IDocketFollowUpLogic
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationLogic _notificationLogic;
        private readonly DocketSettings _settings;

        public DocketFollowUpLogic(ApplicationDbContext context, INotificationLogic notificationLogic, IOptions<DocketSettings> settings)
        {
            _context = context;
            _notificationLogic = notificationLogic;
            _settings = settings.Value;
        }

        public async Task<int> SendOverdueAlertsAsync(CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var activityCutoff = now.AddDays(-_settings.FeedbackOverdueDays);
            var repeatCutoff = now.AddDays(-_settings.OverdueAlertRepeatDays);

            var overdue = await _context.Dockets
                .Where(d => d.StatusId == EDocketStatus.Open || d.StatusId == EDocketStatus.UnderInvestigation)
                .Where(d => d.LastOverdueAlertDate == null || d.LastOverdueAlertDate < repeatCutoff)
                .Select(d => new
                {
                    Docket = d,
                    LastActivity = d.NavDiaryEntries.Max(e => (DateTime?)e.CreatedDate) ?? d.OpenedDate
                })
                .Where(x => x.LastActivity < activityCutoff)
                .ToListAsync(ct);

            foreach (var item in overdue)
            {
                var docket = item.Docket;
                var days = (int)(now - item.LastActivity).TotalDays;
                var message = $"Docket #{docket.Id} has had no investigation diary entry for {days} days. Please record progress and update the complainant.";

                var recipientIds = await _context.Users
                    .Where(u => u.RoleId == ERole.StationCommander && u.StationId == docket.StationId && u.IsActive)
                    .Select(u => u.Id)
                    .ToListAsync(ct);
                if (docket.InvestigatingOfficerId != null)
                {
                    recipientIds.Add(docket.InvestigatingOfficerId.Value);
                }

                foreach (var userId in recipientIds.Distinct())
                {
                    await _notificationLogic.CreateAsync(userId, docket.Id, message);
                }

                docket.LastOverdueAlertDate = now;
            }

            await _context.SaveChangesAsync(ct);
            return overdue.Count;
        }
    }
}
