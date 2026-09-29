using Database.Entities;
using Logic.DTO;
using Logic.Model;

namespace Logic.BusinessLogicInterface
{
    public interface IDocketDiaryLogic
    {
        Task<ResultModel> AddAsync(long docketId, long authorId, DocketDiaryEntryDTO dto, string? ipAddress);

        Task<IEnumerable<DocketDiaryEntry>> GetByDocketIdAsync(long docketId, bool sharedOnly);

        Task<DateTime> GetLastActivityAsync(Docket docket);
    }
}
