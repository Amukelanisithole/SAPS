using Logic.BusinessLogicInterface;
using Logic.Model.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Logic.Enums.Enums;

namespace SAPS.Controllers
{
    // Cookie-based sign-in for the browser-facing site. This is deliberately
    // separate from AuthController (api/Auth), which issues JWTs for external
    // API clients — same IAuthService underneath, different session mechanism.
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IPasswordResetLogic _passwordResetLogic;
        private readonly IEmailVerificationLogic _emailVerificationLogic;

        public AccountController(IAuthService authService, IPasswordResetLogic passwordResetLogic, IEmailVerificationLogic emailVerificationLogic)
        {
            _authService = authService;
            _passwordResetLogic = passwordResetLogic;
            _emailVerificationLogic = emailVerificationLogic;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginRequest());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest request, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var result = await _authService.LoginAsync(request);
            if (result.ResultId != EResultValue.Success || result.Data == null)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(request);
            }

            await SignInWithCookieAsync(result.Data.User);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(new RegisterRequest());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var result = await _authService.RegisterAsync(request, BuildVerificationLink);
            if (result.ResultId != EResultValue.Success || result.Data == null)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(request);
            }

            await SignInWithCookieAsync(result.Data.User);
            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Must resolve HttpContext.User via the same (Cookie) scheme the page that
            // rendered this form used — otherwise antiforgery sees "authenticated" at
            // render time and "anonymous" (the default JWT scheme, with no bearer
            // token on this request) at submit time, and rejects it with a 400.
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordRequest());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            await _passwordResetLogic.RequestResetAsync(request.Email, BuildResetLink);

            // Same view regardless of whether the email existed — the message
            // itself is worded generically so this can't be used to enumerate
            // registered accounts.
            ViewBag.Submitted = true;
            return View(request);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string? email = null, string? token = null)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                return RedirectToAction(nameof(ForgotPassword));
            }

            return View(new ResetPasswordRequest { Email = email, Token = token });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var result = await _passwordResetLogic.ResetPasswordAsync(request);
            if (result.ResultId != EResultValue.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(request);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail(string? email = null, string? token = null)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                return RedirectToAction(nameof(Login));
            }

            var result = await _emailVerificationLogic.VerifyEmailAsync(email, token);
            ViewBag.Success = result.ResultId == EResultValue.Success;
            ViewBag.Message = result.Message;
            return View();
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendVerificationEmail()
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _emailVerificationLogic.SendVerificationEmailAsync(userId, BuildVerificationLink);
            TempData["SuccessMessage"] = "A new verification email has been sent.";
            return RedirectToAction("Index", "Dashboard");
        }

        private string BuildResetLink(string email, string token) =>
            Url.Action(nameof(ResetPassword), "Account", new { email, token }, Request.Scheme)!;

        private string BuildVerificationLink(string email, string token) =>
            Url.Action(nameof(VerifyEmail), "Account", new { email, token }, Request.Scheme)!;

        private async Task SignInWithCookieAsync(UserInfo user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("FullName", user.FullName),
                new Claim(ClaimTypes.Role, user.RoleName),
                new Claim("RoleId", user.RoleId.ToString()),
                new Claim("StationId", user.StationId?.ToString() ?? string.Empty),
                new Claim("EmailVerified", user.EmailVerified ? "true" : "false")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        }
    }
}
