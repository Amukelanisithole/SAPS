using Logic.BusinessLogicInterface;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SAPS.Controllers
{
    [Route("Admin/Affidavits")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "SystemAdministrator")]
    public class AdminAffidavitsController : Controller
    {
        private const int PageSize = 10;

        private readonly IAffidavitLogic _affidavitLogic;
        private readonly IStationLogic _stationLogic;

        public AdminAffidavitsController(IAffidavitLogic affidavitLogic, IStationLogic stationLogic)
        {
            _affidavitLogic = affidavitLogic;
            _stationLogic = stationLogic;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(int page = 1, long? statusId = null, long? stationId = null)
        {
            var affidavits = await _affidavitLogic.GetAllPagedAsync(page, PageSize, statusId, stationId);
            ViewBag.StatusId = statusId;
            ViewBag.StationId = stationId;
            ViewBag.Stations = await _stationLogic.GetAllAsync();
            return View(affidavits);
        }
    }
}
