namespace Logic.DTO
{
    public class ReportTableDTO
    {
        public string Title { get; set; } = string.Empty;
        public string[] Headers { get; set; } = Array.Empty<string>();
        public List<string[]> Rows { get; set; } = new();
    }
}
