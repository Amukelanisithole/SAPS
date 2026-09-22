using Logic.BusinessLogicInterface;
using Logic.DTO;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Logic.Enums.Enums;

namespace SAPS.Controllers
{
    // "Admin" prefix avoids colliding with the existing api/Station JSON controller.
    [Route("Admin/Stations")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "SystemAdministrator")]
    public class AdminStationsController : Controller
    {
        private readonly IStationLogic _stationLogic;

        public AdminStationsController(IStationLogic stationLogic)
        {
            _stationLogic = stationLogic;
        }

        private const int PageSize = 10;

        [HttpGet("")]
        public async Task<IActionResult> Index(int page = 1, string? search = null)
        {
            var stations = await _stationLogic.GetAllPagedAsync(page, PageSize, search);
            ViewBag.Search = search;
            return View(stations);
        }

        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View(new StationDTO());
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StationDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var result = await _stationLogic.CreateAsync(dto);
            if (result.ResultId != EResultValue.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(dto);
            }

            TempData["SuccessMessage"] = "Station created successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
