using Logic.DTO;

namespace Logic.BusinessLogicInterface
{
    public interface ICsvExportService
    {
        byte[] Generate(ReportTableDTO table);
    }
}
