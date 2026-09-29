using Logic.DTO;

namespace Logic.BusinessLogicInterface
{
    public interface IAdminReportLogic
    {
        Task<ReportTableDTO?> GetReportAsync(string reportType);
    }
}
