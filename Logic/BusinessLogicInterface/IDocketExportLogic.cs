using Logic.Model;

namespace Logic.BusinessLogicInterface
{
    public interface IDocketExportLogic
    {
        Task<ResultModel<(byte[] Bytes, string FileName)>> ExportPdfAsync(long docketId, long userId, string? ipAddress);
    }
}
