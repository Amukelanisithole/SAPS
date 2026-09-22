using Logic.BusinessLogicInterface;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SAPS.Controllers
{
    // "Admin" prefix mirrors AdminAffidavits — a system-wide browse view so
    // an administrator can see every emergency alert ever raised, not just
    // the ones currently active at a given station.
    [Route("Admin/EmergencyAlerts")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "SystemAdministrator")]
    public class AdminEmergencyAlertsController : Controller
    {
        private const int PageSize = 10;

        private readonly IEmergencyAlertLogic _emergencyAlertLogic;
        private readonly IStationLogic _stationLogic;

        public AdminEmergencyAlertsController(IEmergencyAlertLogic emergencyAlertLogic, IStationLogic stationLogic)
        {
            _emergencyAlertLogic = emergencyAlertLogic;
            _stationLogic = stationLogic;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(int page = 1, long? statusId = null, long? stationId = null)
        {
            var alerts = await _emergencyAlertLogic.GetAllPagedAsync(page, PageSize, statusId, stationId);
            ViewBag.StatusId = statusId;
            ViewBag.StationId = stationId;
            ViewBag.Stations = await _stationLogic.GetAllAsync();
            return View(alerts);
        }
    }
}
