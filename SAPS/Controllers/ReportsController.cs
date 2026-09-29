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
        private readonly IReferralLogic _referralLogic;

        public ReportsController(IPreliminaryReportLogic preliminaryReportLogic, IDocketLogic docketLogic, IReferralLogic referralLogic)
        {
            _referralLogic = referralLogic;
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
            var isAdmin = User.GetRoleName() == "SystemAdministrator";
            var myStationId = User.GetStationId();
            var canSeeAsStaff = false;
            if (User.GetRoleName() is ("SapsOfficer" or "StationCommander") && myStationId != null)
            {
                canSeeAsStaff = report.NearestStationId == null
                    || report.NearestStationId == myStationId
                    || report.NavDocket?.StationId == myStationId;
                if (!canSeeAsStaff && report.NavDocket != null)
                {
                    var pendingReferral = await _referralLogic.GetPendingForDocketAsync(report.NavDocket.Id);
                    canSeeAsStaff = pendingReferral?.ToStationId == myStationId;
                }
            }

            if (!isOwner && !isAdmin && !canSeeAsStaff)
            {
                return Forbid();
            }

            var isOfficerForReport = User.GetRoleName() == "SapsOfficer"
                && (report.NearestStationId == null || report.NearestStationId == User.GetStationId());
            var isOpenForReview = report.StatusId is (EReportStatus.Submitted or EReportStatus.UnderReview);

            return View(new ReportDetailsViewModel
            {
                Report = report,
                CanConvert = isOfficerForReport && isOpenForReview,
                CanStartReview = isOfficerForReport && report.StatusId == EReportStatus.Submitted,
                CanReject = isOfficerForReport && isOpenForReview
            });
        }

        [HttpPost]
        [Authorize(Roles = "SapsOfficer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartReview(long id)
        {
            var result = await _preliminaryReportLogic.StartReviewAsync(id, User.GetUserId());
            TempData[result.ResultId == EResultValue.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [Authorize(Roles = "SapsOfficer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(long id, ReportRejectDTO reject)
        {
            var result = await _preliminaryReportLogic.RejectAsync(id, User.GetUserId(), reject);
            if (result.ResultId != EResultValue.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(ForReview));
        }

        [HttpPost]
        [Authorize(Roles = "SapsOfficer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Convert(long id, DocketConvertDTO convert)
        {
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
