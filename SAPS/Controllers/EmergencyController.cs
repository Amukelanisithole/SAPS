using Logic.BusinessLogicInterface;
using Logic.DTO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPS.Extensions;
using static Logic.Enums.Enums;

namespace SAPS.Controllers
{
    public class EmergencyController : Controller
    {
        private readonly IEmergencyAlertLogic _emergencyAlertLogic;

        public EmergencyController(IEmergencyAlertLogic emergencyAlertLogic)
        {
            _emergencyAlertLogic = emergencyAlertLogic;
        }

        public async Task<IActionResult> Index()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (authenticateResult.Succeeded && authenticateResult.Principal != null)
            {
                HttpContext.User = authenticateResult.Principal;
            }

            return View(new EmergencyAlertDTO());
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "Complainant")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Alert(EmergencyAlertDTO dto)
        {
            var result = await _emergencyAlertLogic.SubmitAsync(User.GetUserId(), dto, HttpContext.Connection.RemoteIpAddress?.ToString());
            TempData[result.ResultId == EResultValue.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        private const int PageSize = 10;

        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "SapsOfficer,StationCommander")]
        public async Task<IActionResult> ForStation(int page = 1)
        {
            var stationId = User.GetStationId();
            if (stationId == null)
            {
                TempData["ErrorMessage"] = "Your account is not assigned to a station.";
                return RedirectToAction("Index", "Dashboard");
            }

            var alerts = await _emergencyAlertLogic.GetForStationPagedAsync(stationId.Value, page, PageSize);
            return View(alerts);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "SapsOfficer,StationCommander")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Acknowledge(long id)
        {
            var result = await _emergencyAlertLogic.AcknowledgeAsync(User.GetUserId(), id, HttpContext.Connection.RemoteIpAddress?.ToString());
            TempData[result.ResultId == EResultValue.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(ForStation));
        }
    }
}
