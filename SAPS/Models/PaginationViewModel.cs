namespace SAPS.Models
{
    // Feeds the shared _Pagination partial. RouteValues carries whatever
    // filters the calling page currently has applied (e.g. a status or
    // station filter) so paging links preserve them instead of resetting
    // the list back to "all" when the visitor moves between pages.
    public class PaginationViewModel
    {
        public required string Controller { get; set; }
        public required string Action { get; set; }
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public Dictionary<string, string> RouteValues { get; set; } = new();
    }
}
