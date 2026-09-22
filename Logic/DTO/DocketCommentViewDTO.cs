namespace Logic.DTO
{
    public class DocketCommentViewDTO
    {
        public long Id { get; set; }
        public long AuthorId { get; set; }
        public string AuthorFullName { get; set; } = string.Empty;
        public string AuthorRoleName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}
