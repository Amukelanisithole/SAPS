using System.Net;
using System.Net.Mail;
using Logic.BusinessLogicInterface;
using Logic.Model.Auth;
using Microsoft.Extensions.Options;

namespace Logic.BusinessLogic
{
    // Used when Smtp:Host is configured (see Program.cs's conditional
    // registration). Talks to a real mail server, so this is what actually
    // delivers password-reset links once an operator sets up SMTP credentials.
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpSettings _settings;

        public SmtpEmailSender(IOptions<SmtpSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendAsync(string toAddress, string subject, string body, CancellationToken ct = default)
        {
            using var message = new MailMessage
            {
                From = new MailAddress(_settings.FromAddress, _settings.FromName),
                Subject = subject,
                Body = body,
                // Every caller now routes through EmailTemplateService, whose
                // templates are always HTML — see Database/SQL/MainScript/MainScript.sql.
                IsBodyHtml = true
            };
            message.To.Add(toAddress);

            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.EnableSsl
            };

            if (!string.IsNullOrEmpty(_settings.Username))
            {
                client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);
            }

            await client.SendMailAsync(message, ct);
        }
    }
}
