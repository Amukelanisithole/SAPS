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
    public class DocketsController : Controller
    {
        private readonly IDocketLogic _docketLogic;
        private readonly IReferralLogic _referralLogic;
        private readonly IEscalationLogic _escalationLogic;
        private readonly IStationLogic _stationLogic;
        private readonly IDocketCommentLogic _docketCommentLogic;

        public DocketsController(
            IDocketLogic docketLogic,
            IReferralLogic referralLogic,
            IEscalationLogic escalationLogic,
            IStationLogic stationLogic,
            IDocketCommentLogic docketCommentLogic)
        {
            _docketLogic = docketLogic;
            _referralLogic = referralLogic;
            _escalationLogic = escalationLogic;
            _stationLogic = stationLogic;
            _docketCommentLogic = docketCommentLogic;
        }

        private const int PageSize = 10;

        [Authorize(Roles = "Complainant")]
        public async Task<IActionResult> Mine(int page = 1, long? statusId = null)
        {
            var dockets = await _docketLogic.GetForComplainantPagedAsync(User.GetUserId(), page, PageSize, statusId);
            ViewBag.StatusId = statusId;
            return View(dockets);
        }

        [Authorize(Roles = "SapsOfficer,StationCommander")]
        public async Task<IActionResult> Station(int page = 1, long? statusId = null)
        {
            var stationId = User.GetStationId();
            if (stationId == null)
            {
                TempData["ErrorMessage"] = "Your account is not assigned to a station.";
                return RedirectToAction("Index", "Dashboard");
            }

            var dockets = await _docketLogic.GetForStationPagedAsync(stationId.Value, page, PageSize, statusId);
            ViewBag.StatusId = statusId;
            return View(dockets);
        }

        public async Task<IActionResult> Details(long id)
        {
            var docket = await _docketLogic.GetByIdAsync(id, User.GetUserId(), HttpContext.Connection.RemoteIpAddress?.ToString());
            if (docket == null)
            {
                return NotFound();
            }

            var isOwner = docket.NavReport != null && docket.NavReport.ComplainantId == User.GetUserId();
            var isAdmin = User.GetRoleName() == "SystemAdministrator";
            var isOwnStationStaff = User.GetRoleName() is "SapsOfficer" or "StationCommander" && docket.StationId == User.GetStationId();
            if (!isOwner && !isAdmin && !isOwnStationStaff)
            {
                return Forbid();
            }

            var referrals = await _referralLogic.GetByDocketIdAsync(id);
            var escalations = await _escalationLogic.GetByDocketIdAsync(id);
            var comments = await _docketCommentLogic.GetByDocketIdAsync(id);
            var isOfficerAtStation = User.GetRoleName() == "SapsOfficer" && docket.StationId == User.GetStationId();
            var isCommanderAtStation = User.GetRoleName() == "StationCommander" && docket.StationId == User.GetStationId();

            return View(new DocketDetailsViewModel
            {
                Docket = docket,
                Referrals = referrals,
                Escalations = escalations,
                Comments = comments,
                ViewingUserId = User.GetUserId(),
                AvailableStations = isOfficerAtStation
                    ? (await _stationLogic.GetAllAsync()).Where(s => s.Id != docket.StationId)
                    : Enumerable.Empty<Database.Entities.Station>(),
                CanUpdateStatus = isOfficerAtStation || isCommanderAtStation,
                CanRefer = isOfficerAtStation,
                CanEscalate = isOwner,
                // Case messaging is between the complainant and the station working
                // their case — a system administrator isn't a party to the case, so
                // (consistent with the privacy policy's "admins don't routinely
                // access case content") they can view but not post here.
                CanComment = isOwner || isOwnStationStaff
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(long id, DocketCommentDTO newComment)
        {
            var docket = await _docketLogic.GetByIdAsync(id, User.GetUserId(), HttpContext.Connection.RemoteIpAddress?.ToString());
            if (docket == null)
            {
                return NotFound();
            }

            var isOwner = docket.NavReport != null && docket.NavReport.ComplainantId == User.GetUserId();
            var isOwnStationStaff = User.GetRoleName() is "SapsOfficer" or "StationCommander" && docket.StationId == User.GetStationId();
            if (!isOwner && !isOwnStationStaff)
            {
                return Forbid();
            }

            newComment.DocketId = id;
            if (!string.IsNullOrWhiteSpace(newComment.Message))
            {
                await _docketCommentLogic.AddAsync(User.GetUserId(), newComment);
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [Authorize(Roles = "SapsOfficer,StationCommander")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(long id, DocketStatusUpdateDTO statusUpdate)
        {
            // The form posts fields prefixed "StatusUpdate." (DocketDetailsViewModel.StatusUpdate),
            // so this parameter must be named to match that prefix for model binding to populate it.
            var result = await _docketLogic.UpdateStatusAsync(id, User.GetUserId(), statusUpdate, HttpContext.Connection.RemoteIpAddress?.ToString());
            TempData[result.ResultId == EResultValue.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [Authorize(Roles = "SapsOfficer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Refer(long id, ReferralDTO referral)
        {
            // The form posts fields prefixed "Referral." (DocketDetailsViewModel.Referral),
            // so this parameter must be named to match that prefix for model binding to populate it.
            referral.DocketId = id;
            var result = await _referralLogic.CreateAsync(User.GetUserId(), referral, HttpContext.Connection.RemoteIpAddress?.ToString());
            TempData[result.ResultId == EResultValue.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;

            // A successful referral moves the docket to the destination station,
            // so the referring officer immediately loses station-scoped access to
            // its Details page — redirecting back there would be a dead-end 403.
            // Send them to their own station's list instead, where the docket
            // (correctly) no longer appears.
            if (result.ResultId == EResultValue.Success)
            {
                return RedirectToAction(nameof(Station));
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
