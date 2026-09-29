using System.Security.Cryptography;
using System.Text;
using Database.AppDBContext;
using Database.Entities;
using Logic.BusinessLogicInterface;
using Logic.Model;
using Logic.Model.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static Logic.Enums.Enums;

namespace Logic.BusinessLogic
{
    public class PasswordResetLogic : IPasswordResetLogic
    {
        private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(1);
        private const string GenericRequestMessage = "If an account with that email exists, a password reset link has been sent to it.";

        private readonly ApplicationDbContext _context;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IAuditLogWriter _auditLogWriter;
        private readonly AppSettings _appSettings;

        public PasswordResetLogic(ApplicationDbContext context, IEmailTemplateService emailTemplateService, IAuditLogWriter auditLogWriter, IOptions<AppSettings> appSettings)
        {
            _context = context;
            _emailTemplateService = emailTemplateService;
            _auditLogWriter = auditLogWriter;
            _appSettings = appSettings.Value;
        }

        public async Task<ResultModel> RequestResetAsync(string email, Func<string, string, string> buildResetLink, CancellationToken ct = default)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

            if (user == null || !user.IsActive)
            {
                return new ResultModel { ResultId = EResultValue.Success, Message = GenericRequestMessage };
            }

            var rawToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

            _context.PasswordResetTokens.Add(new PasswordResetToken
            {
                UserId = user.Id,
                TokenHash = HashToken(rawToken),
                ExpiresAt = DateTime.UtcNow.Add(TokenLifetime),
                Used = false,
                CreatedDate = DateTime.UtcNow
            });
            await _context.SaveChangesAsync(ct);

            var link = buildResetLink(user.Email, rawToken);
            var baseUrl = _appSettings.BaseUrl?.TrimEnd('/') ?? string.Empty;
            await _emailTemplateService.SendAsync(EEmailTemplates.PasswordReset, user.Email, new Dictionary<string, string>
            {
                ["FullName"] = user.FullName,
                ["ResetLink"] = link,
                ["ExpiryHours"] = TokenLifetime.TotalHours.ToString("0"),
                ["LogoUrl"] = string.IsNullOrEmpty(baseUrl) ? string.Empty : $"{baseUrl}/branding/saps-logo.png"
            }, ct);

            await _auditLogWriter.WriteAsync(new AuditLog
            {
                UserId = user.Id,
                ActionType = "PasswordResetRequested",
                Timestamp = DateTime.UtcNow
            }, ct);

            return new ResultModel { ResultId = EResultValue.Success, Message = GenericRequestMessage };
        }

        public async Task<ResultModel> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
        {
            const string invalidMessage = "This reset link is invalid or has expired. Please request a new one.";

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email, ct);
            if (user == null)
            {
                return new ResultModel { ResultId = EResultValue.Fail, Message = invalidMessage };
            }

            var tokenHash = HashToken(request.Token);
            var resetToken = await _context.PasswordResetTokens
                .Where(t => t.UserId == user.Id && t.TokenHash == tokenHash && !t.Used && t.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(t => t.CreatedDate)
                .FirstOrDefaultAsync(ct);

            if (resetToken == null)
            {
                return new ResultModel { ResultId = EResultValue.Fail, Message = invalidMessage };
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;
            resetToken.Used = true;
            await _context.SaveChangesAsync(ct);

            await _auditLogWriter.WriteAsync(new AuditLog
            {
                UserId = user.Id,
                ActionType = "PasswordReset",
                Timestamp = DateTime.UtcNow
            }, ct);

            return new ResultModel { ResultId = EResultValue.Success, Message = "Your password has been reset. You can now log in." };
        }

        private static string HashToken(string rawToken) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
    }
}
