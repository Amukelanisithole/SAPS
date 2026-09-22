using Logic.DTO;

namespace Logic.BusinessLogicInterface
{
    public interface IPdfExportService
    {
        byte[] Generate(ReportTableDTO table);
    }
}
