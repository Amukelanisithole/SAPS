using Database.Entities;
using Logic.DTO;
using Logic.Model;

namespace Logic.BusinessLogicInterface
{
    public interface IAffidavitLogic
    {
        Task<ResultModel<Affidavit>> SubmitAsync(long complainantId, AffidavitDTO dto, string? ipAddress = null);
        Task<Affidavit?> GetByIdAsync(long id);
        Task<IEnumerable<Affidavit>> GetAllAsync();
        Task<IEnumerable<Affidavit>> GetMineAsync(long complainantId);
        Task<IEnumerable<Affidavit>> GetForReviewAsync(long stationId);
        Task<PagedResult<Affidavit>> GetAllPagedAsync(int page, int pageSize, long? statusId = null, long? stationId = null, CancellationToken ct = default);
        Task<PagedResult<Affidavit>> GetMinePagedAsync(long complainantId, int page, int pageSize, long? statusId = null, CancellationToken ct = default);
        Task<PagedResult<Affidavit>> GetForReviewPagedAsync(long stationId, int page, int pageSize, CancellationToken ct = default);
        Task<ResultModel<Affidavit>> StampAsync(long officerId, AffidavitStampDTO dto, string? ipAddress = null);
        Task<ResultModel<Affidavit>> RejectAsync(long officerId, long affidavitId, string reason, string? ipAddress = null);
        Task<byte[]?> GetVerificationPhotoAsync(long affidavitId);
        Task<(byte[] Bytes, string ContentType)?> GetIdDocumentAsync(long affidavitId);
        Task<byte[]?> GetStampedPdfAsync(long affidavitId);

        Task LogAccessAsync(long affidavitId, long userId, string actionType, string? ipAddress = null);
    }
}
