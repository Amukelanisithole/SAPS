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
    // No controller-level [Authorize] — every action here is anonymous by
    // default (matching HomeController), since the real emergency numbers
    // Index leads with must be reachable by anyone, logged in or not. Each
    // action that actually needs a role opts in with its own [Authorize];
    // a class-level [AllowAnonymous] would short-circuit those per-action
    // checks entirely (see ASP0026), so it's deliberately left off here.
    public class EmergencyController : Controller
    {
        private readonly IEmergencyAlertLogic _emergencyAlertLogic;

        public EmergencyController(IEmergencyAlertLogic emergencyAlertLogic)
        {
            _emergencyAlertLogic = emergencyAlertLogic;
        }

        // Reachable by anonymous visitors, so this can't carry an [Authorize]
        // attribute — but the app's default authentication scheme is JWT
        // Bearer (see Program.cs), so without forcing a cookie-scheme
        // authenticate here, HttpContext.User would stay anonymous even for
        // a browser sending a valid site cookie, and the view's "are you
        // logged in as a Complainant" check below would always say no.
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
