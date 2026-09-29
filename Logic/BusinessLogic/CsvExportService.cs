using System.Text;
using Logic.BusinessLogicInterface;
using Logic.DTO;

namespace Logic.BusinessLogic
{
    public class CsvExportService : ICsvExportService
    {
        public byte[] Generate(ReportTableDTO table)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(',', table.Headers.Select(Escape)));
            foreach (var row in table.Rows)
            {
                sb.AppendLine(string.Join(',', row.Select(Escape)));
            }

            var preamble = Encoding.UTF8.GetPreamble();
            var body = Encoding.UTF8.GetBytes(sb.ToString());
            var result = new byte[preamble.Length + body.Length];
            preamble.CopyTo(result, 0);
            body.CopyTo(result, preamble.Length);
            return result;
        }

        private static string Escape(string? value)
        {
            value ??= string.Empty;
            var mustQuote = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
            if (!mustQuote) return value;
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
    }
}
