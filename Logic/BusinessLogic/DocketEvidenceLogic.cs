using Database.AppDBContext;
using Database.Entities;
using Logic.BusinessLogicInterface;
using Logic.DTO;
using Logic.Model;
using Microsoft.EntityFrameworkCore;
using static Logic.Enums.Enums;

namespace Logic.BusinessLogic
{
    public class DocketEvidenceLogic : IDocketEvidenceLogic
    {
        private const string EvidenceSubfolder = "dockets/evidence";

        public const int MaxFilesPerUpload = 10;
        public const long MaxFileBytes = 10 * 1024 * 1024;

        private readonly ApplicationDbContext _context;
        private readonly IFileStorageService _fileStorageService;
        private readonly IAuditLogWriter _auditLogWriter;
        private readonly INotificationLogic _notificationLogic;

        public DocketEvidenceLogic(ApplicationDbContext context, IFileStorageService fileStorageService, IAuditLogWriter auditLogWriter, INotificationLogic notificationLogic)
        {
            _context = context;
            _fileStorageService = fileStorageService;
            _auditLogWriter = auditLogWriter;
            _notificationLogic = notificationLogic;
        }

        public async Task<ResultModel> AddAsync(long docketId, long uploadedByUserId, DocketEvidenceDTO dto, string? ipAddress)
        {
            try
            {
                var docket = await _context.Dockets
                    .Include(d => d.NavReport)
                    .FirstOrDefaultAsync(d => d.Id == docketId);
                if (docket == null)
                {
                    return Fail("Docket not found.");
                }

                var uploader = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == uploadedByUserId);
                var isStaff = uploader != null && uploader.RoleId is (ERole.SapsOfficer or ERole.StationCommander) && uploader.StationId == docket.StationId;
                var isComplainant = uploader != null && uploader.RoleId == ERole.Complainant && docket.NavReport?.ComplainantId == uploader.Id;
                if (!isStaff && !isComplainant)
                {
                    return Fail("Only the complainant or staff at the docket's station can add evidence.");
                }

                if (EDocketStatus.IsFinal(docket.StatusId))
                {
                    return Fail($"This docket is {EDocketStatus.GetName(docket.StatusId)} — evidence can no longer be added.");
                }

                if (string.IsNullOrWhiteSpace(dto.Description))
                {
                    return Fail("Please describe the evidence.");
                }

                if (dto.Files.Count == 0)
                {
                    return Fail("Please choose at least one file.");
                }

                if (dto.Files.Count > MaxFilesPerUpload)
                {
                    return Fail($"You can upload at most {MaxFilesPerUpload} files at a time.");
                }

                foreach (var file in dto.Files)
                {
                    if (file.Bytes.Length == 0 || file.Bytes.Length > MaxFileBytes)
                    {
                        return Fail($"{file.FileName} must be between 1 byte and {MaxFileBytes / (1024 * 1024)} MB.");
                    }

                    if (DetectContentType(file.Bytes) == null)
                    {
                        return Fail($"{file.FileName} is not a JPG, PNG or PDF file.");
                    }
                }

                var saved = new List<DocketEvidence>();
                foreach (var file in dto.Files)
                {
                    var path = await _fileStorageService.SaveAsync(EvidenceSubfolder, file.FileName, file.Bytes);
                    saved.Add(new DocketEvidence
                    {
                        DocketId = docketId,
                        UploadedByUserId = uploadedByUserId,
                        Description = dto.Description.Trim(),
                        FilePath = path,
                        OriginalFileName = Path.GetFileName(file.FileName),
                        ContentType = DetectContentType(file.Bytes)!,
                        FileSizeBytes = file.Bytes.Length,
                        UploadedDate = DateTime.UtcNow,
                        SubmittedByComplainant = isComplainant,
                        ReviewStatusId = isStaff ? EEvidenceReviewStatus.Accepted : EEvidenceReviewStatus.Pending,
                        ReviewedByUserId = isStaff ? uploadedByUserId : null,
                        ReviewedDate = isStaff ? DateTime.UtcNow : null
                    });
                }

                _context.DocketEvidence.AddRange(saved);
                docket.LastUpdated = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var plural = saved.Count == 1 ? "" : "s";
                await _auditLogWriter.WriteAsync(new AuditLog
                {
                    DocketId = docketId,
                    UserId = uploadedByUserId,
                    ActionType = isStaff ? $"EvidenceAdded ({saved.Count} file{plural})" : $"EvidenceSubmittedByComplainant ({saved.Count} file{plural})",
                    Timestamp = DateTime.UtcNow,
                    IpAddress = ipAddress
                });

                if (isComplainant)
                {
                    await NotifyReviewersAsync(docket, $"The complainant submitted {saved.Count} evidence file{plural} on docket #{docketId} for review.");
                    return new ResultModel { ResultId = EResultValue.Success, Message = $"{saved.Count} file{plural} submitted. An officer will review {(saved.Count == 1 ? "it" : "them")}." };
                }

                return new ResultModel { ResultId = EResultValue.Success, Message = $"{saved.Count} evidence file{plural} added." };
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while adding evidence: {ex.Message}");
            }
        }

        public async Task<ResultModel> ReviewAsync(long evidenceId, long reviewerId, bool accept, string? note, string? ipAddress)
        {
            try
            {
                var evidence = await _context.DocketEvidence
                    .Include(e => e.NavDocket).ThenInclude(d => d!.NavReport)
                    .FirstOrDefaultAsync(e => e.Id == evidenceId);
                if (evidence?.NavDocket == null)
                {
                    return Fail("Evidence not found.");
                }

                var docket = evidence.NavDocket;
                var reviewer = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == reviewerId);
                if (reviewer == null || reviewer.RoleId is not (ERole.SapsOfficer or ERole.StationCommander) || reviewer.StationId != docket.StationId)
                {
                    return Fail("Only staff at the docket's station can review evidence.");
                }

                if (evidence.ReviewStatusId != EEvidenceReviewStatus.Pending)
                {
                    return Fail("This evidence has already been reviewed.");
                }

                note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
                if (!accept && note == null)
                {
                    return Fail("Please give a reason for dismissing this evidence.");
                }

                evidence.ReviewStatusId = accept ? EEvidenceReviewStatus.Accepted : EEvidenceReviewStatus.Dismissed;
                evidence.ReviewedByUserId = reviewerId;
                evidence.ReviewedDate = DateTime.UtcNow;
                evidence.ReviewNote = note;
                docket.LastUpdated = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                await _auditLogWriter.WriteAsync(new AuditLog
                {
                    DocketId = docket.Id,
                    UserId = reviewerId,
                    ActionType = $"Evidence{(accept ? "Accepted" : "Dismissed")} (#{evidence.Id})",
                    Timestamp = DateTime.UtcNow,
                    IpAddress = ipAddress
                });

                if (docket.NavReport != null)
                {
                    await _notificationLogic.CreateAsync(docket.NavReport.ComplainantId, docket.Id, accept
                        ? $"The evidence you submitted on docket #{docket.Id} ({evidence.OriginalFileName}) has been accepted."
                        : $"The evidence you submitted on docket #{docket.Id} ({evidence.OriginalFileName}) was not accepted. Reason: {note}");
                }

                return new ResultModel { ResultId = EResultValue.Success, Message = accept ? "Evidence accepted." : "Evidence dismissed." };
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while reviewing the evidence: {ex.Message}");
            }
        }

        private async Task NotifyReviewersAsync(Docket docket, string message)
        {
            var recipientIds = new List<long>();
            if (docket.InvestigatingOfficerId != null)
            {
                recipientIds.Add(docket.InvestigatingOfficerId.Value);
            }
            else
            {
                recipientIds.AddRange(await _context.Users
                    .Where(u => u.RoleId == ERole.StationCommander && u.StationId == docket.StationId && u.IsActive)
                    .Select(u => u.Id)
                    .ToListAsync());
            }

            foreach (var userId in recipientIds)
            {
                await _notificationLogic.CreateAsync(userId, docket.Id, message);
            }
        }

        public async Task<int> CountPendingForInvestigatorAsync(long officerId)
        {
            return await _context.DocketEvidence
                .CountAsync(e => e.ReviewStatusId == EEvidenceReviewStatus.Pending
                    && e.NavDocket != null && e.NavDocket.InvestigatingOfficerId == officerId);
        }

        public async Task<IEnumerable<DocketEvidence>> GetByDocketIdAsync(long docketId)
        {
            return await _context.DocketEvidence
                .Include(e => e.NavUploadedBy)
                .Include(e => e.NavReviewedBy)
                .Where(e => e.DocketId == docketId)
                .OrderBy(e => e.UploadedDate)
                .ThenBy(e => e.Id)
                .ToListAsync();
        }

        public async Task<(DocketEvidence Evidence, byte[] Bytes)?> GetFileAsync(long evidenceId)
        {
            var evidence = await _context.DocketEvidence
                .Include(e => e.NavDocket)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == evidenceId);
            if (evidence == null) return null;

            var bytes = await _fileStorageService.ReadAsync(evidence.FilePath);
            return bytes == null ? null : (evidence, bytes);
        }

        public static string? DetectContentType(byte[] bytes)
        {
            if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
                return "image/jpeg";
            if (bytes.Length >= 8 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47
                && bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A)
                return "image/png";
            if (bytes.Length >= 5 && bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46 && bytes[4] == 0x2D)
                return "application/pdf";
            return null;
        }

        private static ResultModel Fail(string message) =>
            new() { ResultId = EResultValue.Fail, Message = message };
    }
}
