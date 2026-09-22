using Logic.DTO;
using Logic.Model;

namespace Logic.BusinessLogicInterface
{
    public interface IDocketCommentLogic
    {
        Task<IEnumerable<DocketCommentViewDTO>> GetByDocketIdAsync(long docketId);
        Task<ResultModel> AddAsync(long authorId, DocketCommentDTO dto);
    }
}
