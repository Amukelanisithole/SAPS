using Database.AppDBContext;
using Database.Entities;
using Logic.BusinessLogicInterface;
using Logic.Model;
using Logic.Model.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static Logic.Enums.Enums;

namespace Logic.BusinessLogic
{
    public class AuthService : IAuthService
    {
        // After this many consecutive failed attempts, the account is locked
        // for LockoutDuration — slows down brute-force guessing without
        // requiring a CAPTCHA or external rate-limiting infrastructure.
        private const int MaxFailedAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IAuditLogWriter _auditLogWriter;
        private readonly IEmailVerificationLogic _emailVerificationLogic;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            ApplicationDbContext context,
            IJwtService jwtService,
            IAuditLogWriter auditLogWriter,
            IEmailVerificationLogic emailVerificationLogic,
            ILogger<AuthService> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _auditLogWriter = auditLogWriter;
            _emailVerificationLogic = emailVerificationLogic;
            _logger = logger;
        }

        public async Task<ResultModel<LoginResponse>> RegisterAsync(RegisterRequest request, Func<string, string, string> buildVerificationLink)
        {
            try
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email || u.IdNumber == request.IdNumber);

                if (existingUser != null)
                {
                    return new ResultModel<LoginResponse> { ResultId = EResultValue.Fail, Message = "An account with this email or ID number already exists." };
                }

                var user = new User
                {
                    FullName = request.FullName,
                    IdNumber = request.IdNumber,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    RoleId = ERole.Complainant,
                    StationId = null,
                    CreatedDate = DateTime.UtcNow,
                    // Self-registration is the only path that requires the
                    // verify-by-email flow — see User.EmailVerified.
                    EmailVerified = false
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                await _auditLogWriter.WriteAsync(new AuditLog
                {
                    UserId = user.Id,
                    ActionType = "Registered",
                    Timestamp = DateTime.UtcNow
                });

                try
                {
                    await _emailVerificationLogic.SendVerificationEmailAsync(user.Id, buildVerificationLink);
                }
                catch (Exception ex)
                {
                    // A failed verification email should never block account creation.
                    _logger.LogWarning(ex, "Failed to send verification email to {Email}.", user.Email);
                }

                return await LoginAsync(new LoginRequest { Email = request.Email, Password = request.Password });
            }
            catch (Exception ex)
            {
                return new ResultModel<LoginResponse> { ResultId = EResultValue.Fail, Message = $"An error occurred during registration: {ex.Message}" };
            }
        }

        public async Task<ResultModel<LoginResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

                if (user != null && user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
                {
                    var minutesLeft = Math.Ceiling((user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes);
                    return new ResultModel<LoginResponse>
                    {
                        ResultId = EResultValue.Fail,
                        Message = $"This account is temporarily locked due to repeated failed login attempts. Try again in {minutesLeft:0} minute(s), or reset your password."
                    };
                }

                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    if (user != null)
                    {
                        user.FailedLoginAttempts++;
                        if (user.FailedLoginAttempts >= MaxFailedAttempts)
                        {
                            user.LockoutEnd = DateTime.UtcNow.Add(LockoutDuration);
                        }
                        await _context.SaveChangesAsync();
                    }

                    return new ResultModel<LoginResponse> { ResultId = EResultValue.Fail, Message = "Invalid email or password." };
                }

                if (!user.IsActive)
                {
                    return new ResultModel<LoginResponse> { ResultId = EResultValue.Fail, Message = "This account has been deactivated. Please contact an administrator." };
                }

                user.FailedLoginAttempts = 0;
                user.LockoutEnd = null;
                user.LastLogin = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var roleName = ERole.GetName(user.RoleId);
                var token = _jwtService.GenerateJwtToken(user, roleName);

                await _auditLogWriter.WriteAsync(new AuditLog
                {
                    UserId = user.Id,
                    ActionType = "Login",
                    Timestamp = DateTime.UtcNow
                });

                return new ResultModel<LoginResponse>
                {
                    ResultId = EResultValue.Success,
                    Message = "Login successful.",
                    Data = new LoginResponse
                    {
                        Token = token,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                        User = new UserInfo
                        {
                            Id = user.Id,
                            FullName = user.FullName,
                            Email = user.Email,
                            RoleId = user.RoleId,
                            RoleName = roleName,
                            StationId = user.StationId,
                            EmailVerified = user.EmailVerified
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResultModel<LoginResponse> { ResultId = EResultValue.Fail, Message = $"An error occurred during login: {ex.Message}" };
            }
        }
    }
}
