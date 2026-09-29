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
        private readonly IDocketEvidenceLogic _docketEvidenceLogic;
        private readonly IDocketExportLogic _docketExportLogic;
        private readonly IAuditLogWriter _auditLogWriter;
        private readonly IDocketDiaryLogic _docketDiaryLogic;
        private readonly Logic.Model.DocketSettings _docketSettings;
        private readonly IPreliminaryReportLogic _preliminaryReportLogic;

        public DocketsController(
            IDocketLogic docketLogic,
            IReferralLogic referralLogic,
            IEscalationLogic escalationLogic,
            IStationLogic stationLogic,
            IDocketCommentLogic docketCommentLogic,
            IDocketEvidenceLogic docketEvidenceLogic,
            IDocketExportLogic docketExportLogic,
            IAuditLogWriter auditLogWriter,
            IDocketDiaryLogic docketDiaryLogic,
            Microsoft.Extensions.Options.IOptions<Logic.Model.DocketSettings> docketSettings,
            IPreliminaryReportLogic preliminaryReportLogic)
        {
            _preliminaryReportLogic = preliminaryReportLogic;
            _docketSettings = docketSettings.Value;
            _docketDiaryLogic = docketDiaryLogic;
            _docketLogic = docketLogic;
            _referralLogic = referralLogic;
            _escalationLogic = escalationLogic;
            _stationLogic = stationLogic;
            _docketCommentLogic = docketCommentLogic;
            _docketEvidenceLogic = docketEvidenceLogic;
            _docketExportLogic = docketExportLogic;
            _auditLogWriter = auditLogWriter;
        }

        private string? IpAddress => HttpContext.Connection.RemoteIpAddress?.ToString();

        private const int PageSize = 10;

        [Authorize(Roles = "Complainant")]
        public async Task<IActionResult> Mine(int page = 1, long? statusId = null)
        {
            var dockets = await _docketLogic.GetForComplainantPagedAsync(User.GetUserId(), page, PageSize, statusId);
            ViewBag.StatusId = statusId;
            return View(dockets);
        }

        [Authorize(Roles = "SapsOfficer")]
        public async Task<IActionResult> Assigned(int page = 1, bool includeFinished = false)
        {
            var officerId = User.GetUserId();
            return View(new MyCasesViewModel
            {
                Dockets = await _docketLogic.GetAssignedPagedAsync(officerId, page, PageSize, includeFinished),
                ReportsUnderReview = await _preliminaryReportLogic.GetUnderReviewByOfficerAsync(officerId),
                IncludeFinished = includeFinished,
                OverdueThresholdDays = _docketSettings.FeedbackOverdueDays
            });
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
            ViewBag.IncomingReferrals = await _referralLogic.GetPendingIncomingAsync(stationId.Value);
            return View(dockets);
        }

        public async Task<IActionResult> Details(long id)
        {
            var docket = await _docketLogic.GetByIdAsync(id, User.GetUserId(), IpAddress, includeDetails: true);
            if (docket == null)
            {
                return NotFound();
            }

            var isOwner = docket.NavReport != null && docket.NavReport.ComplainantId == User.GetUserId();
            var isAdmin = User.GetRoleName() == "SystemAdministrator";
            var isOwnStationStaff = IsStationStaff(docket.StationId);
            var pendingReferral = await _referralLogic.GetPendingForDocketAsync(id);
            var isReceivingStaff = pendingReferral != null && IsStationStaff(pendingReferral.ToStationId);
            if (!isOwner && !isAdmin && !isOwnStationStaff && !isReceivingStaff)
            {
                return Forbid();
            }

            var referrals = await _referralLogic.GetByDocketIdAsync(id);
            var escalations = await _escalationLogic.GetByDocketIdAsync(id);
            var comments = await _docketCommentLogic.GetByDocketIdAsync(id);
            var history = await _docketLogic.GetStatusHistoryAsync(id);
            var isOfficerAtStation = User.GetRoleName() == "SapsOfficer" && docket.StationId == User.GetStationId();
            var isCommanderAtStation = User.GetRoleName() == "StationCommander" && docket.StationId == User.GetStationId();
            var isFinal = EDocketStatus.IsFinal(docket.StatusId);
            var transitions = isOwnStationStaff
                ? EDocketStatus.GetManualTransitions(docket.StatusId, isCommanderAtStation)
                : Array.Empty<long>();
            var canSeeCaseFile = isOwnStationStaff || isReceivingStaff;

            var allEvidence = await _docketEvidenceLogic.GetByDocketIdAsync(id);
            int? daysSinceActivity = null;
            if (isOwnStationStaff && docket.StatusId is (EDocketStatus.Open or EDocketStatus.UnderInvestigation))
            {
                daysSinceActivity = (int)(DateTime.UtcNow - await _docketDiaryLogic.GetLastActivityAsync(docket)).TotalDays;
            }

            return View(new DocketDetailsViewModel
            {
                Docket = docket,
                Referrals = referrals,
                Escalations = escalations,
                Comments = comments,
                StatusHistory = history,
                Evidence = canSeeCaseFile
                    ? allEvidence
                    : allEvidence.Where(e => isOwner && e.UploadedByUserId == User.GetUserId()),
                Diary = await _docketDiaryLogic.GetByDocketIdAsync(id, sharedOnly: !canSeeCaseFile),
                PendingReferral = pendingReferral,
                DaysSinceActivity = daysSinceActivity,
                OverdueThresholdDays = _docketSettings.FeedbackOverdueDays,
                IsReceivingStaff = isReceivingStaff,
                CanRespondToReferral = pendingReferral != null && User.GetRoleName() == "StationCommander" && pendingReferral.ToStationId == User.GetStationId(),
                CanCancelReferral = pendingReferral != null && isOwnStationStaff,
                CanAddDiary = isOwnStationStaff && !isFinal,
                CanReviewEvidence = isOwnStationStaff && !isFinal,
                CanSubmitEvidence = isOwner && !isFinal,
                AssignableOfficers = isCommanderAtStation && !isFinal
                    ? await _docketLogic.GetAssignableOfficersAsync(docket.StationId)
                    : Enumerable.Empty<Database.Entities.User>(),
                ManualTransitions = transitions,
                ViewingUserId = User.GetUserId(),
                AvailableStations = isOfficerAtStation
                    ? (await _stationLogic.GetAllAsync()).Where(s => s.Id != docket.StationId)
                    : Enumerable.Empty<Database.Entities.Station>(),
                IsStationStaff = isOwnStationStaff,
                IsCommander = isCommanderAtStation,
                CanUpdateStatus = transitions.Count > 0,
                CanRefer = isOfficerAtStation && docket.StatusId is (EDocketStatus.Open or EDocketStatus.UnderInvestigation),
                CanEscalate = isOwner,
                CanComment = (isOwner || isOwnStationStaff) && !isFinal,
                CanEditCase = isOwnStationStaff && !isFinal,
                CanAddEvidence = isOwnStationStaff && !isFinal,
                CanRequestClosure = (isCommanderAtStation || (isOfficerAtStation && docket.InvestigatingOfficerId == User.GetUserId()))
                    && docket.StatusId is (EDocketStatus.Open or EDocketStatus.UnderInvestigation),
                CanApproveClosure = isCommanderAtStation && docket.StatusId == EDocketStatus.PendingClosure,
                CanDownload = isOwnStationStaff,
                CasUpdate = new() { CasNumber = docket.CasNumber ?? string.Empty },
                Assign = new() { OfficerId = docket.InvestigatingOfficerId ?? 0 }
            });
        }

        private bool IsStationStaff(long docketStationId) =>
            User.GetRoleName() is "SapsOfficer" or "StationCommander" && docketStationId == User.GetStationId();

        [HttpPost]
        [Authorize(Roles = "SapsOfficer,StationCommander")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDiaryEntry(long id, DocketDiaryEntryDTO newDiaryEntry)
        {
            if (!ModelState.IsValid) return RedirectWithResult(id, EResultValue.Fail, FirstModelError());

            var result = await _docketDiaryLogic.AddAsync(id, User.GetUserId(), newDiaryEntry, IpAddress);
            return RedirectWithResult(id, result.ResultId, result.Message);
        }

        [HttpPost]
        [Authorize(Roles = "SapsOfficer,StationCommander")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReviewEvidence(long id, long evidenceId, bool accept, string? note)
        {
            var result = await _docketEvidenceLogic.ReviewAsync(evidenceId, User.GetUserId(), accept, note, IpAddress);
            return RedirectWithResult(id, result.ResultId, result.Message);
        }

        [HttpPost]
        [Authorize(Roles = "StationCommander")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptReferral(long id, long referralId)
        {
            var result = await _referralLogic.AcceptAsync(referralId, User.GetUserId(), IpAddress);
            return RedirectWithResult(id, result.ResultId, result.Message);
        }

        [HttpPost]
        [Authorize(Roles = "StationCommander")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeclineReferral(long id, long referralId, string? reason)
        {
            var result = await _referralLogic.DeclineAsync(referralId, User.GetUserId(), reason ?? string.Empty, IpAddress);
            if (result.ResultId != EResultValue.Success)
            {
                return RedirectWithResult(id, result.ResultId, result.Message);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Station));
        }

        [HttpPost]
        [Authorize(Roles = "SapsOfficer,StationCommander")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelReferral(long id, long referralId)
        {
            var result = await _referralLogic.CancelAsync(referralId, User.GetUserId(), IpAddress);
            return RedirectWithResult(id, result.ResultId, result.Message);
        }

        private IActionResult RedirectWithResult(long id, long resultId, string message)
        {
            TempData[resultId == EResultValue.Success ? "SuccessMessage" : "ErrorMessage"] = message;
            return RedirectToAction(nameof(Details), new { id });
        }

        private string FirstModelError() =>
            ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).FirstOrDefault(m => !string.IsNullOrWhiteSpace(m))
            ?? "Please check the form and try again.";

        [HttpPost]
        [Authorize(Roles = "SapsOfficer,StationCommander")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetCasNumber(long id, DocketCasNumberDTO casUpdate)
        {
            if (!ModelState.IsValid) return RedirectWithResult(id, EResultValue.Fail, FirstModelError());

            var result = await _docketLogic.SetCasNumberAsync(id, User.GetUserId(), casUpdate, IpAddress);
            return RedirectWithResult(id, result.ResultId, result.Message);
        }

        [HttpPost]
        [Authorize(Roles = "StationCommander")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignOfficer(long id, DocketAssignDTO assign)
        {
            if (!ModelState.IsValid) return RedirectWithResult(id, EResultValue.Fail, FirstModelError());

            var result = await _docketLogic.AssignInvestigatingOfficerAsync(id, User.GetUserId(), assign, IpAddress);
            return RedirectWithResult(id, result.ResultId, result.Message);
        }

        [HttpPost]
        [Authorize(Roles = "SapsOfficer,StationCommander")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestClosure(long id, DocketClosureDTO closure)
        {
            if (!ModelState.IsValid) return RedirectWithResult(id, EResultValue.Fail, FirstModelError());

            var result = await _docketLogic.RequestClosureAsync(id, User.GetUserId(), closure, IpAddress);
            return RedirectWithResult(id, result.ResultId, result.Message);
        }

        [HttpPost]
        [Authorize(Roles = "StationCommander")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveClosure(long id)
        {
            var result = await _docketLogic.ApproveClosureAsync(id, User.GetUserId(), IpAddress);
            return RedirectWithResult(id, result.ResultId, result.Message);
        }

        [HttpPost]
        [Authorize(Roles = "SapsOfficer,StationCommander,Complainant")]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(105 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 105 * 1024 * 1024)]
        public async Task<IActionResult> AddEvidence(long id, DocketEvidenceDTO newEvidence, List<IFormFile> evidenceFiles)
        {
            if (!ModelState.IsValid) return RedirectWithResult(id, EResultValue.Fail, FirstModelError());

            foreach (var file in evidenceFiles.Where(f => f.Length > 0))
            {
                if (file.Length > Logic.BusinessLogic.DocketEvidenceLogic.MaxFileBytes)
                {
                    return RedirectWithResult(id, EResultValue.Fail, $"{file.FileName} is larger than 10 MB.");
                }

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                newEvidence.Files.Add(new EvidenceFileDTO
                {
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    Bytes = stream.ToArray()
                });
            }

            var result = await _docketEvidenceLogic.AddAsync(id, User.GetUserId(), newEvidence, IpAddress);
            return RedirectWithResult(id, result.ResultId, result.Message);
        }

        public async Task<IActionResult> Evidence(long id, bool download = false)
        {
            var file = await _docketEvidenceLogic.GetFileAsync(id);
            if (file?.Evidence.NavDocket == null)
            {
                return NotFound();
            }

            var (evidence, bytes) = file.Value;
            var allowed = IsStationStaff(evidence.NavDocket!.StationId)
                || (User.GetRoleName() == "Complainant" && evidence.UploadedByUserId == User.GetUserId());
            if (!allowed)
            {
                var pendingReferral = await _referralLogic.GetPendingForDocketAsync(evidence.DocketId);
                allowed = pendingReferral != null && IsStationStaff(pendingReferral.ToStationId);
            }

            if (!allowed)
            {
                return Forbid();
            }

            Response.Headers["X-Content-Type-Options"] = "nosniff";

            if (!download)
            {
                return File(bytes, evidence.ContentType);
            }

            await _auditLogWriter.WriteAsync(new Database.Entities.AuditLog
            {
                DocketId = evidence.DocketId,
                UserId = User.GetUserId(),
                ActionType = $"EvidenceDownloaded (#{evidence.Id})",
                Timestamp = DateTime.UtcNow,
                IpAddress = IpAddress
            });

            return File(bytes, evidence.ContentType, evidence.OriginalFileName);
        }

        [Authorize(Roles = "SapsOfficer,StationCommander")]
        public async Task<IActionResult> DownloadPdf(long id)
        {
            var result = await _docketExportLogic.ExportPdfAsync(id, User.GetUserId(), IpAddress);
            if (result.ResultId != EResultValue.Success)
            {
                return RedirectWithResult(id, result.ResultId, result.Message);
            }

            return File(result.Data.Bytes, "application/pdf", result.Data.FileName);
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
                var result = await _docketCommentLogic.AddAsync(User.GetUserId(), newComment);
                if (result.ResultId != EResultValue.Success)
                {
                    TempData["ErrorMessage"] = result.Message;
                }
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [Authorize(Roles = "SapsOfficer,StationCommander")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(long id, DocketStatusUpdateDTO statusUpdate)
        {
            var result = await _docketLogic.UpdateStatusAsync(id, User.GetUserId(), statusUpdate, HttpContext.Connection.RemoteIpAddress?.ToString());
            TempData[result.ResultId == EResultValue.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [Authorize(Roles = "SapsOfficer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Refer(long id, ReferralDTO referral)
        {
            referral.DocketId = id;
            var result = await _referralLogic.CreateAsync(User.GetUserId(), referral, HttpContext.Connection.RemoteIpAddress?.ToString());
            return RedirectWithResult(id, result.ResultId, result.Message);
        }
    }
}
