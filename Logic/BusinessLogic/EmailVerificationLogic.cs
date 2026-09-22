using System.Security.Cryptography;
using System.Text;
using Database.AppDBContext;
using Database.Entities;
using Logic.BusinessLogicInterface;
using Logic.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static Logic.Enums.Enums;

namespace Logic.BusinessLogic
{
    public class EmailVerificationLogic : IEmailVerificationLogic
    {
        private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(24);

        private readonly ApplicationDbContext _context;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IAuditLogWriter _auditLogWriter;
        private readonly AppSettings _appSettings;

        public EmailVerificationLogic(
            ApplicationDbContext context,
            IEmailTemplateService emailTemplateService,
            IAuditLogWriter auditLogWriter,
            IOptions<AppSettings> appSettings)
        {
            _context = context;
            _emailTemplateService = emailTemplateService;
            _auditLogWriter = auditLogWriter;
            _appSettings = appSettings.Value;
        }

        public async Task SendVerificationEmailAsync(long userId, Func<string, string, string> buildVerificationLink, CancellationToken ct = default)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
            if (user == null || user.EmailVerified) return;

            var rawToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

            _context.EmailVerificationTokens.Add(new EmailVerificationToken
            {
                UserId = user.Id,
                TokenHash = HashToken(rawToken),
                ExpiresAt = DateTime.UtcNow.Add(TokenLifetime),
                Used = false,
                CreatedDate = DateTime.UtcNow
            });
            await _context.SaveChangesAsync(ct);

            var baseUrl = _appSettings.BaseUrl?.TrimEnd('/') ?? string.Empty;
            await _emailTemplateService.SendAsync(EEmailTemplates.Welcome, user.Email, new Dictionary<string, string>
            {
                ["FullName"] = user.FullName,
                ["LoginLink"] = string.IsNullOrEmpty(baseUrl) ? string.Empty : $"{baseUrl}/Account/Login",
                ["VerificationLink"] = buildVerificationLink(user.Email, rawToken),
                ["LogoUrl"] = string.IsNullOrEmpty(baseUrl) ? string.Empty : $"{baseUrl}/branding/saps-logo.png"
            }, ct);
        }

        public async Task<ResultModel> VerifyEmailAsync(string email, string token, CancellationToken ct = default)
        {
            const string invalidMessage = "This verification link is invalid or has expired. Request a new one below.";

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
            if (user == null)
            {
                return new ResultModel { ResultId = EResultValue.Fail, Message = invalidMessage };
            }

            if (user.EmailVerified)
            {
                return new ResultModel { ResultId = EResultValue.Success, Message = "Your email address is already verified." };
            }

            var tokenHash = HashToken(token);
            var verificationToken = await _context.EmailVerificationTokens
                .Where(t => t.UserId == user.Id && t.TokenHash == tokenHash && !t.Used && t.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(t => t.CreatedDate)
                .FirstOrDefaultAsync(ct);

            if (verificationToken == null)
            {
                return new ResultModel { ResultId = EResultValue.Fail, Message = invalidMessage };
            }

            user.EmailVerified = true;
            verificationToken.Used = true;
            await _context.SaveChangesAsync(ct);

            await _auditLogWriter.WriteAsync(new AuditLog
            {
                UserId = user.Id,
                ActionType = "EmailVerified",
                Timestamp = DateTime.UtcNow
            }, ct);

            return new ResultModel { ResultId = EResultValue.Success, Message = "Your email address has been verified." };
        }

        private static string HashToken(string rawToken) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
    }
}
