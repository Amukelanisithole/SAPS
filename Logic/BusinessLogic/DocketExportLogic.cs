using Database.AppDBContext;
using Database.Entities;
using Logic.BusinessLogicInterface;
using Logic.Model;
using Microsoft.EntityFrameworkCore;
using static Logic.Enums.Enums;

namespace Logic.BusinessLogic
{
    public class DocketExportLogic : IDocketExportLogic
    {
        private readonly ApplicationDbContext _context;
        private readonly IDocketLogic _docketLogic;
        private readonly IDocketEvidenceLogic _docketEvidenceLogic;
        private readonly IDocketCommentLogic _docketCommentLogic;
        private readonly IReferralLogic _referralLogic;
        private readonly IEscalationLogic _escalationLogic;
        private readonly IFileStorageService _fileStorageService;
        private readonly IDocketPdfService _docketPdfService;
        private readonly IAuditLogWriter _auditLogWriter;
        private readonly IDocketDiaryLogic _docketDiaryLogic;

        public DocketExportLogic(
            ApplicationDbContext context,
            IDocketLogic docketLogic,
            IDocketEvidenceLogic docketEvidenceLogic,
            IDocketCommentLogic docketCommentLogic,
            IReferralLogic referralLogic,
            IEscalationLogic escalationLogic,
            IFileStorageService fileStorageService,
            IDocketPdfService docketPdfService,
            IAuditLogWriter auditLogWriter,
            IDocketDiaryLogic docketDiaryLogic)
        {
            _docketDiaryLogic = docketDiaryLogic;
            _context = context;
            _docketLogic = docketLogic;
            _docketEvidenceLogic = docketEvidenceLogic;
            _docketCommentLogic = docketCommentLogic;
            _referralLogic = referralLogic;
            _escalationLogic = escalationLogic;
            _fileStorageService = fileStorageService;
            _docketPdfService = docketPdfService;
            _auditLogWriter = auditLogWriter;
        }

        public async Task<ResultModel<(byte[] Bytes, string FileName)>> ExportPdfAsync(long docketId, long userId, string? ipAddress)
        {
            try
            {
                var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
                var docket = await _context.Dockets
                    .AsNoTracking()
                    .Include(d => d.NavReport).ThenInclude(r => r!.NavComplainant)
                    .Include(d => d.NavStation)
                    .Include(d => d.NavReviewingOfficer)
                    .Include(d => d.NavInvestigatingOfficer)
                    .Include(d => d.NavClosedBy)
                    .FirstOrDefaultAsync(d => d.Id == docketId);

                if (docket == null)
                {
                    return Fail("Docket not found.");
                }

                if (user == null || user.RoleId is not (ERole.SapsOfficer or ERole.StationCommander) || user.StationId != docket.StationId)
                {
                    return Fail("Only officers and commanders at the docket's station can download it.");
                }

                var evidenceItems = new List<DocketPdfEvidenceItem>();
                foreach (var evidence in (await _docketEvidenceLogic.GetByDocketIdAsync(docketId)).Where(e => e.ReviewStatusId == EEvidenceReviewStatus.Accepted))
                {
                    var isImage = evidence.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
                    evidenceItems.Add(new DocketPdfEvidenceItem
                    {
                        Evidence = evidence,
                        ImageBytes = isImage ? await _fileStorageService.ReadAsync(evidence.FilePath) : null
                    });
                }

                var model = new DocketPdfModel
                {
                    Docket = docket,
                    StatusHistory = await _docketLogic.GetStatusHistoryAsync(docketId),
                    Diary = (await _docketDiaryLogic.GetByDocketIdAsync(docketId, sharedOnly: false)).Reverse().ToList(),
                    Referrals = await _referralLogic.GetByDocketIdAsync(docketId),
                    Escalations = await _escalationLogic.GetByDocketIdAsync(docketId),
                    Comments = await _docketCommentLogic.GetByDocketIdAsync(docketId),
                    Evidence = evidenceItems,
                    GeneratedByName = user.FullName,
                    GeneratedAt = DateTime.UtcNow
                };

                var bytes = _docketPdfService.Generate(model);

                await _auditLogWriter.WriteAsync(new AuditLog
                {
                    DocketId = docketId,
                    UserId = userId,
                    ActionType = "DocketPdfDownloaded",
                    Timestamp = DateTime.UtcNow,
                    IpAddress = ipAddress
                });

                var fileName = docket.CasNumber != null
                    ? $"Docket-{docket.Id}-{SanitizeFileName(docket.CasNumber)}.pdf"
                    : $"Docket-{docket.Id}.pdf";

                return new ResultModel<(byte[] Bytes, string FileName)> { ResultId = EResultValue.Success, Message = "Successful", Data = (bytes, fileName) };
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while generating the docket PDF: {ex.Message}");
            }
        }

        private static string SanitizeFileName(string value)
        {
            var invalid = Path.GetInvalidFileNameChars().Append('/').Append(' ').ToHashSet();
            return new string(value.Select(c => invalid.Contains(c) ? '-' : c).ToArray());
        }

        private static ResultModel<(byte[] Bytes, string FileName)> Fail(string message) =>
            new() { ResultId = EResultValue.Fail, Message = message };
    }
}
