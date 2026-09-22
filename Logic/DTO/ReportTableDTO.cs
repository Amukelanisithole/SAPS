namespace Logic.DTO
{
    // A flattened, already-formatted table — deliberately not typed per report
    // kind, so both the CSV and PDF exporters (and the picker page) can work
    // off one shape no matter which of the system's entities it came from.
    public class ReportTableDTO
    {
        public string Title { get; set; } = string.Empty;
        public string[] Headers { get; set; } = Array.Empty<string>();
        public List<string[]> Rows { get; set; } = new();
    }
}
