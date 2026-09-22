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
    public class AffidavitsController : Controller
    {
        // 8 MB — comfortably covers a webcam/phone-camera JPEG capture.
        private const long MaxVerificationPhotoBytes = 8 * 1024 * 1024;
        // 8 MB also covers a phone photo or a single-page scanned PDF of an ID document.
        private const long MaxIdDocumentBytes = 8 * 1024 * 1024;
        private static readonly string[] AllowedIdDocumentContentTypes = { "image/jpeg", "image/png", "application/pdf" };

        private readonly IAffidavitLogic _affidavitLogic;

        public AffidavitsController(IAffidavitLogic affidavitLogic)
        {
            _affidavitLogic = affidavitLogic;
        }

        [Authorize(Roles = "Complainant")]
        public IActionResult Submit()
        {
            return View(new AffidavitDTO { FullName = User.GetFullName() });
        }

        [HttpPost]
        [Authorize(Roles = "Complainant")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(AffidavitDTO dto, IFormFile? verificationPhoto, IFormFile? idDocument)
        {
            if (verificationPhoto == null || verificationPhoto.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "A photo is required to confirm your identity.");
            }
            else if (verificationPhoto.Length > MaxVerificationPhotoBytes)
            {
                ModelState.AddModelError(string.Empty, "The identity photo is too large.");
            }

            if (idDocument == null || idDocument.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "A photo or scan of your ID document is required.");
            }
            else if (idDocument.Length > MaxIdDocumentBytes)
            {
                ModelState.AddModelError(string.Empty, "The ID document is too large.");
            }
            else if (!AllowedIdDocumentContentTypes.Contains(idDocument.ContentType))
            {
                ModelState.AddModelError(string.Empty, "The ID document must be a JPG, PNG, or PDF file.");
            }

            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            using (var stream = new MemoryStream())
            {
                await verificationPhoto!.CopyToAsync(stream);
                dto.VerificationPhotoBytes = stream.ToArray();
                dto.VerificationPhotoFileName = verificationPhoto.FileName;
            }

            using (var stream = new MemoryStream())
            {
                await idDocument!.CopyToAsync(stream);
                dto.IdDocumentBytes = stream.ToArray();
                dto.IdDocumentFileName = idDocument.FileName;
                dto.IdDocumentContentType = idDocument.ContentType;
            }

            var result = await _affidavitLogic.SubmitAsync(User.GetUserId(), dto, HttpContext.Connection.RemoteIpAddress?.ToString());
            if (result.ResultId != EResultValue.Success || result.Data == null)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(dto);
            }

            TempData["SuccessMessage"] = $"Affidavit submitted. Your reference number is {result.Data.ReferenceNumber}.";
            return RedirectToAction(nameof(Details), new { id = result.Data.Id });
        }

        private const int PageSize = 10;

        [Authorize(Roles = "Complainant")]
        public async Task<IActionResult> Mine(int page = 1, long? statusId = null)
        {
            var affidavits = await _affidavitLogic.GetMinePagedAsync(User.GetUserId(), page, PageSize, statusId);
            ViewBag.StatusId = statusId;
            return View(affidavits);
        }

        [Authorize(Roles = "SapsOfficer,StationCommander")]
        public async Task<IActionResult> ForReview(int page = 1)
        {
            var stationId = User.GetStationId();
            if (stationId == null)
            {
                TempData["ErrorMessage"] = "Your account is not assigned to a station.";
                return RedirectToAction("Index", "Dashboard");
            }

            var affidavits = await _affidavitLogic.GetForReviewPagedAsync(stationId.Value, page, PageSize);
            return View(affidavits);
        }

        public async Task<IActionResult> Details(long id)
        {
            var affidavit = await _affidavitLogic.GetByIdAsync(id);
            if (affidavit == null)
            {
                return NotFound();
            }

            var isOwner = affidavit.ComplainantId == User.GetUserId();
            var isStaffAtStation = User.GetRoleName() is "SapsOfficer" or "StationCommander" && affidavit.NearestStationId == User.GetStationId();
            var isAdmin = User.GetRoleName() == "SystemAdministrator";
            if (!isOwner && !isStaffAtStation && !isAdmin)
            {
                return Forbid();
            }

            await _affidavitLogic.LogAccessAsync(affidavit.Id, User.GetUserId(), "AffidavitViewed", HttpContext.Connection.RemoteIpAddress?.ToString());

            return View(new AffidavitDetailsViewModel
            {
                Affidavit = affidavit,
                CanAction = isStaffAtStation && affidavit.StatusId == EAffidavitStatus.Submitted,
                Stamp = new AffidavitStampDTO { AffidavitId = affidavit.Id }
            });
        }

        [HttpPost]
        [Authorize(Roles = "SapsOfficer,StationCommander")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Stamp(long id, AffidavitStampDTO stamp)
        {
            stamp.AffidavitId = id;
            var result = await _affidavitLogic.StampAsync(User.GetUserId(), stamp, HttpContext.Connection.RemoteIpAddress?.ToString());
            TempData[result.ResultId == EResultValue.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [Authorize(Roles = "SapsOfficer,StationCommander")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(long id, string reason)
        {
            var result = await _affidavitLogic.RejectAsync(User.GetUserId(), id, reason, HttpContext.Connection.RemoteIpAddress?.ToString());
            TempData[result.ResultId == EResultValue.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Details), new { id });
        }

        public async Task<IActionResult> DownloadPdf(long id)
        {
            var affidavit = await _affidavitLogic.GetByIdAsync(id);
            if (affidavit == null)
            {
                return NotFound();
            }

            var isOwner = affidavit.ComplainantId == User.GetUserId();
            var isStaffAtStation = User.GetRoleName() is "SapsOfficer" or "StationCommander" && affidavit.NearestStationId == User.GetStationId();
            var isAdmin = User.GetRoleName() == "SystemAdministrator";
            if (!isOwner && !isStaffAtStation && !isAdmin)
            {
                return Forbid();
            }

            var pdfBytes = await _affidavitLogic.GetStampedPdfAsync(id);
            if (pdfBytes == null)
            {
                return NotFound();
            }

            await _affidavitLogic.LogAccessAsync(affidavit.Id, User.GetUserId(), "AffidavitPdfDownloaded", HttpContext.Connection.RemoteIpAddress?.ToString());

            return File(pdfBytes, "application/pdf", $"Affidavit-{affidavit.ReferenceNumber}.pdf");
        }

        public async Task<IActionResult> VerificationPhoto(long id)
        {
            var affidavit = await _affidavitLogic.GetByIdAsync(id);
            if (affidavit == null)
            {
                return NotFound();
            }

            var isOwner = affidavit.ComplainantId == User.GetUserId();
            var isStaffAtStation = User.GetRoleName() is "SapsOfficer" or "StationCommander" && affidavit.NearestStationId == User.GetStationId();
            var isAdmin = User.GetRoleName() == "SystemAdministrator";
            if (!isOwner && !isStaffAtStation && !isAdmin)
            {
                return Forbid();
            }

            var photoBytes = await _affidavitLogic.GetVerificationPhotoAsync(id);
            if (photoBytes == null)
            {
                return NotFound();
            }

            return File(photoBytes, "image/jpeg");
        }

        public async Task<IActionResult> IdDocument(long id)
        {
            var affidavit = await _affidavitLogic.GetByIdAsync(id);
            if (affidavit == null)
            {
                return NotFound();
            }

            var isOwner = affidavit.ComplainantId == User.GetUserId();
            var isStaffAtStation = User.GetRoleName() is "SapsOfficer" or "StationCommander" && affidavit.NearestStationId == User.GetStationId();
            var isAdmin = User.GetRoleName() == "SystemAdministrator";
            if (!isOwner && !isStaffAtStation && !isAdmin)
            {
                return Forbid();
            }

            var document = await _affidavitLogic.GetIdDocumentAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            return File(document.Value.Bytes, document.Value.ContentType);
        }
    }
}
