using Database.AppDBContext;
using Logic.BusinessLogicInterface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logic.BusinessLogic
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<EmailTemplateService> _logger;

        public EmailTemplateService(ApplicationDbContext context, IEmailSender emailSender, ILogger<EmailTemplateService> logger)
        {
            _context = context;
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task SendAsync(long templateId, string toEmail, IDictionary<string, string> placeholders, CancellationToken ct = default)
        {
            var template = await _context.EmailTemplates
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == templateId && t.IsActive, ct);

            if (template == null)
            {
                _logger.LogWarning("Email template {TemplateId} not found or inactive — email to {ToEmail} was not sent.", templateId, toEmail);
                return;
            }

            var subject = Render(template.Subject, placeholders);
            var body = Render(template.Body, placeholders);

            try
            {
                await _emailSender.SendAsync(toEmail, subject, body, ct);
            }
            catch (Exception ex)
            {
                // Every caller (registration, password reset, case
                // notifications) treats email as best-effort, not the
                // source of truth — a transient SMTP failure (rate limits,
                // a bad connection, etc.) must never fail the business
                // action that triggered the email. Centralized here instead
                // of repeated in every caller, since this is the one place
                // all of them funnel through.
                _logger.LogWarning(ex, "Failed to send template {TemplateId} email to {ToEmail}.", templateId, toEmail);
            }
        }

        private static string Render(string text, IDictionary<string, string> placeholders)
        {
            foreach (var (key, value) in placeholders)
            {
                text = text.Replace("{" + key + "}", value);
            }

            return text;
        }
    }
}
