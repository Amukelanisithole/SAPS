using Logic.BusinessLogicInterface;
using Microsoft.Extensions.Logging;

namespace Logic.BusinessLogic
{
    // Registered instead of TwilioSmsSender when no Sms:AccountSid is
    // configured (see Program.cs) — same reasoning as LoggingEmailSender:
    // nothing is actually delivered, but the attempt (and its content) is
    // visible in the log instead of silently vanishing.
    public class LoggingSmsSender : ISmsSender
    {
        private readonly ILogger<LoggingSmsSender> _logger;

        public LoggingSmsSender(ILogger<LoggingSmsSender> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(string toPhoneNumber, string message, CancellationToken ct = default)
        {
            _logger.LogWarning(
                "Sms:AccountSid is not configured, so no SMS was actually sent. Would have sent to {ToPhoneNumber}: {Message}",
                toPhoneNumber, message);
            return Task.CompletedTask;
        }
    }
}
