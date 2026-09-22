using Logic.DTO;

namespace Logic.BusinessLogicInterface
{
    public interface IAdminReportLogic
    {
        // Returns null for an unrecognized type key rather than throwing —
        // the controller treats that as a 404, since it only ever happens
        // from a tampered query string (the picker page only links valid keys).
        Task<ReportTableDTO?> GetReportAsync(string reportType);
    }
}
