using Logic.BusinessLogicInterface;
using Logic.DTO;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPS.Extensions;
using static Logic.Enums.Enums;

namespace SAPS.Controllers
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class EscalationsController : Controller
    {
        private readonly IEscalationLogic _escalationLogic;

        public EscalationsController(IEscalationLogic escalationLogic)
        {
            _escalationLogic = escalationLogic;
        }

        [Authorize(Roles = "Complainant")]
        public IActionResult Create(long docketId)
        {
            return View(new EscalationDTO { DocketId = docketId });
        }

        [HttpPost]
        [Authorize(Roles = "Complainant")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EscalationDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var result = await _escalationLogic.CreateAsync(User.GetUserId(), dto);
            if (result.ResultId != EResultValue.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(dto);
            }

            TempData["SuccessMessage"] = "Escalation lodged. The Station Commander has been notified.";
            return RedirectToAction(nameof(Mine));
        }

        private const int PageSize = 10;

        [Authorize(Roles = "Complainant")]
        public async Task<IActionResult> Mine(int page = 1, long? statusId = null)
        {
            var escalations = await _escalationLogic.GetMinePagedAsync(User.GetUserId(), page, PageSize, statusId);
            ViewBag.StatusId = statusId;
            return View(escalations);
        }

        [Authorize(Roles = "StationCommander")]
        public async Task<IActionResult> Station(int page = 1, long? statusId = null)
        {
            var stationId = User.GetStationId();
            if (stationId == null)
            {
                TempData["ErrorMessage"] = "Your account is not assigned to a station.";
                return RedirectToAction("Index", "Dashboard");
            }

            var escalations = await _escalationLogic.GetForStationPagedAsync(stationId.Value, page, PageSize, statusId);
            ViewBag.StatusId = statusId;
            return View(escalations);
        }

        [HttpPost]
        [Authorize(Roles = "StationCommander")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(long id, EscalationStatusUpdateDTO dto)
        {
            var result = await _escalationLogic.UpdateStatusAsync(id, User.GetUserId(), dto);
            TempData[result.ResultId == EResultValue.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Station));
        }
    }
}
