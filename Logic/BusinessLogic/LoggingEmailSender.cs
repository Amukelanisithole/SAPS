using Logic.BusinessLogicInterface;
using Microsoft.Extensions.Logging;

namespace Logic.BusinessLogic
{
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
