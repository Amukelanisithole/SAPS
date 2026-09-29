namespace SAPS.Models
{
    public class PaginationViewModel
    {
        public required string Controller { get; set; }
        public required string Action { get; set; }
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public Dictionary<string, string> RouteValues { get; set; } = new();
    }
}
