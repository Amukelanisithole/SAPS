using Logic.BusinessLogicInterface;
using Logic.DTO;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPS.Extensions;
using SAPS.Models;
using static Logic.Enums.Enums;

namespace SAPS.Controllers
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class ReportsController : Controller
    {
        private readonly IPreliminaryReportLogic _preliminaryReportLogic;
        private readonly IDocketLogic _docketLogic;

        public ReportsController(IPreliminaryReportLogic preliminaryReportLogic, IDocketLogic docketLogic)
        {
            _preliminaryReportLogic = preliminaryReportLogic;
            _docketLogic = docketLogic;
        }

        [Authorize(Roles = "Complainant")]
        public IActionResult Submit()
        {
            return View(new PreliminaryReportDTO { IncidentDate = DateTime.Now });
        }

        [HttpPost]
        [Authorize(Roles = "Complainant")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(PreliminaryReportDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var result = await _preliminaryReportLogic.SubmitAsync(User.GetUserId(), dto);
            if (result.ResultId != EResultValue.Success || result.Data == null)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(dto);
            }

            TempData["SuccessMessage"] = $"Report submitted. Your reference number is {result.Data.ReferenceNumber}.";
            return RedirectToAction(nameof(Details), new { id = result.Data.Id });
        }

        private const int PageSize = 10;

        [Authorize(Roles = "Complainant")]
        public async Task<IActionResult> Mine(int page = 1, long? statusId = null)
        {
            var reports = await _preliminaryReportLogic.GetMinePagedAsync(User.GetUserId(), page, PageSize, statusId);
            ViewBag.StatusId = statusId;
            return View(reports);
        }

        [Authorize(Roles = "SapsOfficer,StationCommander")]
        public async Task<IActionResult> ForReview(int page = 1, long? statusId = null)
        {
            var stationId = User.GetStationId();
            if (stationId == null)
            {
                TempData["ErrorMessage"] = "Your account is not assigned to a station.";
                return RedirectToAction("Index", "Dashboard");
            }

            var reports = await _preliminaryReportLogic.GetForReviewPagedAsync(stationId.Value, page, PageSize, statusId);
            ViewBag.StatusId = statusId;
            return View(reports);
        }

        public async Task<IActionResult> Details(long id)
        {
            var report = await _preliminaryReportLogic.GetByIdAsync(id);
            if (report == null)
            {
                return NotFound();
            }

            var isOwner = report.ComplainantId == User.GetUserId();
            var isStaff = User.GetRoleName() is "SapsOfficer" or "StationCommander" or "SystemAdministrator";
            if (!isOwner && !isStaff)
            {
                return Forbid();
            }

            return View(new ReportDetailsViewModel
            {
                Report = report,
                CanConvert = User.GetRoleName() == "SapsOfficer" && report.StatusId != EReportStatus.Converted
            });
        }

        [HttpPost]
        [Authorize(Roles = "SapsOfficer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Convert(long id, DocketConvertDTO convert)
        {
            // The form posts fields prefixed "Convert." (it binds ReportDetailsViewModel.Convert),
            // so this parameter must be named to match that prefix for model binding to populate it.
            convert.ReportId = id;
            var result = await _docketLogic.ConvertAsync(User.GetUserId(), convert, HttpContext.Connection.RemoteIpAddress?.ToString());
            if (result.ResultId != EResultValue.Success || result.Data == null)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["SuccessMessage"] = "Docket opened successfully.";
            return RedirectToAction("Details", "Dockets", new { id = result.Data.Id });
        }
    }
}
