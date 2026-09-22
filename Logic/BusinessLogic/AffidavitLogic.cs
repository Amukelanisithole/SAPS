using Database.AppDBContext;
using Database.Entities;
using Logic.BusinessLogicInterface;
using Logic.DTO;
using Logic.Model;
using Microsoft.EntityFrameworkCore;
using static Logic.Enums.Enums;

namespace Logic.BusinessLogic
{
    public class AffidavitLogic : IAffidavitLogic
    {
        private const string PhotoSubfolder = "affidavits/verification-photos";
        private const string IdDocumentSubfolder = "affidavits/id-documents";
        private const string PdfSubfolder = "affidavits/stamped-pdfs";

        private readonly ApplicationDbContext _context;
        private readonly IStationRoutingService _stationRoutingService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IAffidavitPdfService _affidavitPdfService;
        private readonly INotificationLogic _notificationLogic;
        private readonly IAuditLogWriter _auditLogWriter;

        public AffidavitLogic(
            ApplicationDbContext context,
            IStationRoutingService stationRoutingService,
            IFileStorageService fileStorageService,
            IAffidavitPdfService affidavitPdfService,
            INotificationLogic notificationLogic,
            IAuditLogWriter auditLogWriter)
        {
            _context = context;
            _stationRoutingService = stationRoutingService;
            _fileStorageService = fileStorageService;
            _affidavitPdfService = affidavitPdfService;
            _notificationLogic = notificationLogic;
            _auditLogWriter = auditLogWriter;
        }

        public async Task<ResultModel<Affidavit>> SubmitAsync(long complainantId, AffidavitDTO dto, string? ipAddress = null)
        {
            try
            {
                if (dto.Latitude == null || dto.Longitude == null)
                {
                    return new ResultModel<Affidavit> { ResultId = EResultValue.Fail, Message = "Location access is required so your affidavit can be routed to the nearest station. Please enable location and try again." };
                }

                if (dto.VerificationPhotoBytes == null || dto.VerificationPhotoBytes.Length == 0)
                {
                    return new ResultModel<Affidavit> { ResultId = EResultValue.Fail, Message = "A photo is required to confirm your identity." };
                }

                if (dto.IdDocumentBytes == null || dto.IdDocumentBytes.Length == 0)
                {
                    return new ResultModel<Affidavit> { ResultId = EResultValue.Fail, Message = "A photo or scan of your ID document is required." };
                }

                if (dto.ObjectionToOath == null)
                {
                    return new ResultModel<Affidavit> { ResultId = EResultValue.Fail, Message = "Please indicate whether you object to taking the prescribed oath." };
                }

                var nearestStationId = await _stationRoutingService.FindOrCreateNearestStationIdAsync(dto.Latitude.Value, dto.Longitude.Value);
                if (nearestStationId == null)
                {
                    return new ResultModel<Affidavit> { ResultId = EResultValue.Fail, Message = "No SAPS station could be found near your location. Please try again or contact an administrator." };
                }

                var photoPath = await _fileStorageService.SaveAsync(PhotoSubfolder, dto.VerificationPhotoFileName ?? "photo.jpg", dto.VerificationPhotoBytes);
                var idDocumentPath = await _fileStorageService.SaveAsync(IdDocumentSubfolder, dto.IdDocumentFileName ?? "id-document", dto.IdDocumentBytes);

                var affidavit = new Affidavit
                {
                    ComplainantId = complainantId,
                    ReferenceNumber = await GenerateUniqueReferenceNumberAsync(),
                    FullName = dto.FullName,
                    IdNumber = dto.IdNumber,
                    Age = dto.Age,
                    WorkingAddress = dto.WorkingAddress,
                    TelWork = dto.TelWork,
                    TelHome = dto.TelHome,
                    TelCell = dto.TelCell,
                    DeclarationText = dto.DeclarationText,
                    ObjectionToOath = dto.ObjectionToOath.Value,
                    VerificationPhotoPath = photoPath,
                    IdDocumentPath = idDocumentPath,
                    IdDocumentContentType = dto.IdDocumentContentType,
                    SubmissionTimestamp = DateTime.UtcNow,
                    StatusId = EAffidavitStatus.Submitted,
                    ComplainantLatitude = dto.Latitude,
                    ComplainantLongitude = dto.Longitude,
                    NearestStationId = nearestStationId
                };

                _context.Affidavits.Add(affidavit);
                await _context.SaveChangesAsync();

                await LogAccessAsync(affidavit.Id, complainantId, "AffidavitSubmitted", ipAddress);

                return new ResultModel<Affidavit> { ResultId = EResultValue.Success, Message = "Affidavit submitted successfully.", Data = affidavit };
            }
            catch (Exception ex)
            {
                return new ResultModel<Affidavit> { ResultId = EResultValue.Fail, Message = $"An error occurred while submitting the affidavit: {ex.Message}" };
            }
        }

        public async Task<Affidavit?> GetByIdAsync(long id)
        {
            return await _context.Affidavits
                .Include(a => a.NavNearestStation)
                .Include(a => a.NavStampedByOfficer)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Affidavit>> GetAllAsync()
        {
            return await _context.Affidavits
                .Include(a => a.NavComplainant)
                .Include(a => a.NavNearestStation)
                .OrderByDescending(a => a.SubmissionTimestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<Affidavit>> GetMineAsync(long complainantId)
        {
            return await _context.Affidavits
                .Where(a => a.ComplainantId == complainantId)
                .OrderByDescending(a => a.SubmissionTimestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<Affidavit>> GetForReviewAsync(long stationId)
        {
            return await _context.Affidavits
                .Where(a => a.StatusId == EAffidavitStatus.Submitted && a.NearestStationId == stationId)
                .OrderBy(a => a.SubmissionTimestamp)
                .ToListAsync();
        }

        public async Task<PagedResult<Affidavit>> GetAllPagedAsync(int page, int pageSize, long? statusId = null, long? stationId = null, CancellationToken ct = default)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var query = _context.Affidavits
                .Include(a => a.NavComplainant)
                .Include(a => a.NavNearestStation)
                .AsQueryable();

            if (statusId != null)
            {
                query = query.Where(a => a.StatusId == statusId);
            }
            if (stationId != null)
            {
                query = query.Where(a => a.NearestStationId == stationId);
            }

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(a => a.SubmissionTimestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResult<Affidavit> { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount };
        }

        public async Task<PagedResult<Affidavit>> GetMinePagedAsync(long complainantId, int page, int pageSize, long? statusId = null, CancellationToken ct = default)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var query = _context.Affidavits.Where(a => a.ComplainantId == complainantId);
            if (statusId != null)
            {
                query = query.Where(a => a.StatusId == statusId);
            }

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(a => a.SubmissionTimestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResult<Affidavit> { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount };
        }

        public async Task<PagedResult<Affidavit>> GetForReviewPagedAsync(long stationId, int page, int pageSize, CancellationToken ct = default)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var query = _context.Affidavits.Where(a => a.StatusId == EAffidavitStatus.Submitted && a.NearestStationId == stationId);

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderBy(a => a.SubmissionTimestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResult<Affidavit> { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount };
        }

        public async Task<ResultModel<Affidavit>> StampAsync(long officerId, AffidavitStampDTO dto, string? ipAddress = null)
        {
            var affidavit = await _context.Affidavits
                .Include(a => a.NavNearestStation)
                .FirstOrDefaultAsync(a => a.Id == dto.AffidavitId);

            if (affidavit == null)
            {
                return new ResultModel<Affidavit> { ResultId = EResultValue.Fail, Message = "Affidavit not found." };
            }

            if (affidavit.StatusId != EAffidavitStatus.Submitted)
            {
                return new ResultModel<Affidavit> { ResultId = EResultValue.Fail, Message = "This affidavit has already been actioned." };
            }

            var officer = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == officerId);
            if (officer == null || officer.StationId != affidavit.NearestStationId)
            {
                return new ResultModel<Affidavit> { ResultId = EResultValue.Fail, Message = "You may only certify affidavits routed to your own station." };
            }

            affidavit.StampedByOfficerId = officerId;
            affidavit.StampedByOfficerName = officer.FullName;
            affidavit.StampedByOfficerForceNumber = dto.ForceNumber;
            affidavit.StampedByOfficerRank = dto.Rank;
            affidavit.StampedDate = DateTime.UtcNow;
            affidavit.StatusId = EAffidavitStatus.Stamped;

            var photoBytes = await _fileStorageService.ReadAsync(affidavit.VerificationPhotoPath);
            var pdfBytes = _affidavitPdfService.Generate(affidavit, photoBytes);
            affidavit.StampedPdfPath = await _fileStorageService.SaveAsync(PdfSubfolder, $"{affidavit.ReferenceNumber}.pdf", pdfBytes);

            await _context.SaveChangesAsync();

            await LogAccessAsync(affidavit.Id, officerId, "AffidavitStamped", ipAddress);

            await _notificationLogic.CreateAsync(affidavit.ComplainantId, null,
                $"Your affidavit {affidavit.ReferenceNumber} has been certified by a SAPS officer and is ready to download.");

            return new ResultModel<Affidavit> { ResultId = EResultValue.Success, Message = "Affidavit certified successfully.", Data = affidavit };
        }

        public async Task<ResultModel<Affidavit>> RejectAsync(long officerId, long affidavitId, string reason, string? ipAddress = null)
        {
            var affidavit = await _context.Affidavits.FirstOrDefaultAsync(a => a.Id == affidavitId);
            if (affidavit == null)
            {
                return new ResultModel<Affidavit> { ResultId = EResultValue.Fail, Message = "Affidavit not found." };
            }

            if (affidavit.StatusId != EAffidavitStatus.Submitted)
            {
                return new ResultModel<Affidavit> { ResultId = EResultValue.Fail, Message = "This affidavit has already been actioned." };
            }

            var officer = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == officerId);
            if (officer == null || officer.StationId != affidavit.NearestStationId)
            {
                return new ResultModel<Affidavit> { ResultId = EResultValue.Fail, Message = "You may only action affidavits routed to your own station." };
            }

            affidavit.StatusId = EAffidavitStatus.Rejected;
            affidavit.RejectionReason = reason;
            affidavit.StampedByOfficerId = officerId;
            affidavit.StampedByOfficerName = officer.FullName;
            affidavit.StampedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await LogAccessAsync(affidavit.Id, officerId, "AffidavitRejected", ipAddress);

            await _notificationLogic.CreateAsync(affidavit.ComplainantId, null,
                $"Your affidavit {affidavit.ReferenceNumber} was not certified. Reason: {reason}");

            return new ResultModel<Affidavit> { ResultId = EResultValue.Success, Message = "Affidavit rejected.", Data = affidavit };
        }

        public async Task<byte[]?> GetVerificationPhotoAsync(long affidavitId)
        {
            var affidavit = await _context.Affidavits.AsNoTracking().FirstOrDefaultAsync(a => a.Id == affidavitId);
            return affidavit == null ? null : await _fileStorageService.ReadAsync(affidavit.VerificationPhotoPath);
        }

        public async Task<(byte[] Bytes, string ContentType)?> GetIdDocumentAsync(long affidavitId)
        {
            var affidavit = await _context.Affidavits.AsNoTracking().FirstOrDefaultAsync(a => a.Id == affidavitId);
            if (affidavit?.IdDocumentPath == null) return null;

            var bytes = await _fileStorageService.ReadAsync(affidavit.IdDocumentPath);
            return bytes == null ? null : (bytes, affidavit.IdDocumentContentType ?? "application/octet-stream");
        }

        public async Task<byte[]?> GetStampedPdfAsync(long affidavitId)
        {
            var affidavit = await _context.Affidavits.AsNoTracking().FirstOrDefaultAsync(a => a.Id == affidavitId);
            if (affidavit?.StampedPdfPath == null) return null;
            return await _fileStorageService.ReadAsync(affidavit.StampedPdfPath);
        }

        public async Task LogAccessAsync(long affidavitId, long userId, string actionType, string? ipAddress = null)
        {
            await _auditLogWriter.WriteAsync(new AuditLog
            {
                AffidavitId = affidavitId,
                UserId = userId,
                ActionType = actionType,
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress
            });
        }

        // Reference numbers double as the complainant's proof of submission,
        // same convention as PreliminaryReport's, distinguished by prefix.
        private async Task<string> GenerateUniqueReferenceNumberAsync()
        {
            string candidate;
            do
            {
                candidate = $"AFF-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(100000, 999999)}";
            }
            while (await _context.Affidavits.AnyAsync(a => a.ReferenceNumber == candidate));

            return candidate;
        }
    }
}
