using Database.Entities;
using Logic.BusinessLogicInterface;
using Logic.DTO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPS.Extensions;
using SAPS.Models;
using System.Security.Claims;
using static Logic.Enums.Enums;

namespace SAPS.Controllers
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class ProfileController : Controller
    {
        private readonly IUserManagementLogic _userManagementLogic;
        private readonly IEmailVerificationLogic _emailVerificationLogic;

        public ProfileController(IUserManagementLogic userManagementLogic, IEmailVerificationLogic emailVerificationLogic)
        {
            _userManagementLogic = userManagementLogic;
            _emailVerificationLogic = emailVerificationLogic;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await BuildViewModelAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDTO profile)
        {
            if (!ModelState.IsValid)
            {
                return View(nameof(Index), await BuildViewModelAsync(profileOverride: profile));
            }

            var existingUser = await _userManagementLogic.GetByIdAsync(User.GetUserId());
            var emailChanged = existingUser != null && !string.Equals(existingUser.Email, profile.Email, StringComparison.OrdinalIgnoreCase);

            var result = await _userManagementLogic.UpdateOwnProfileAsync(User.GetUserId(), profile);
            if (result.ResultId != EResultValue.Success || result.Data == null)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(nameof(Index), await BuildViewModelAsync(profileOverride: profile));
            }

            if (emailChanged)
            {
                await _emailVerificationLogic.SendVerificationEmailAsync(result.Data.Id, BuildVerificationLink);
            }

            await RefreshSignInAsync(result.Data);

            TempData["SuccessMessage"] = emailChanged
                ? "Profile updated. We've sent a verification link to your new email address."
                : "Profile updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO changePassword)
        {
            if (!ModelState.IsValid)
            {
                return View(nameof(Index), await BuildViewModelAsync(passwordOverride: changePassword));
            }

            var result = await _userManagementLogic.ChangePasswordAsync(User.GetUserId(), changePassword);
            if (result.ResultId != EResultValue.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(nameof(Index), await BuildViewModelAsync(passwordOverride: changePassword));
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        private async Task<ProfileViewModel> BuildViewModelAsync(UpdateProfileDTO? profileOverride = null, ChangePasswordDTO? passwordOverride = null)
        {
            var user = await _userManagementLogic.GetByIdAsync(User.GetUserId());
            return new ProfileViewModel
            {
                Profile = profileOverride ?? new UpdateProfileDTO
                {
                    FullName = user?.FullName ?? string.Empty,
                    Email = user?.Email ?? string.Empty,
                    PhoneNumber = user?.PhoneNumber ?? string.Empty
                },
                ChangePassword = passwordOverride ?? new ChangePasswordDTO()
            };
        }

        private string BuildVerificationLink(string email, string token) =>
            Url.Action("VerifyEmail", "Account", new { email, token }, Request.Scheme)!;

        private async Task RefreshSignInAsync(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("FullName", user.FullName),
                new Claim(ClaimTypes.Role, ERole.GetName(user.RoleId)),
                new Claim("RoleId", user.RoleId.ToString()),
                new Claim("StationId", user.StationId?.ToString() ?? string.Empty),
                new Claim("EmailVerified", user.EmailVerified ? "true" : "false")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        }
    }
}
