using Logic.BusinessLogicInterface;
using Microsoft.Extensions.Logging;

namespace Logic.BusinessLogic
{
    // Registered instead of SmtpEmailSender when no Smtp:Host is configured
    // (see Program.cs). Nothing is actually delivered — this exists so the
    // password-reset flow is still fully testable in a fresh dev environment
    // before anyone has real SMTP credentials to hand, by surfacing the
    // email content (including the reset link) in the application log
    // instead of silently doing nothing.
    public class LoggingEmailSender : IEmailSender
    {
        private readonly ILogger<LoggingEmailSender> _logger;

        public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(string toAddress, string subject, string body, CancellationToken ct = default)
        {
            _logger.LogWarning(
                "Smtp:Host is not configured, so no email was actually sent. Would have sent to {ToAddress} — Subject: {Subject}\n{Body}",
                toAddress, subject, body);
            return Task.CompletedTask;
        }
    }
}
