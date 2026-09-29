namespace Database.Entities
{
    public class PasswordResetToken
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string TokenHash { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool Used { get; set; }
        public DateTime CreatedDate { get; set; }

        public User? NavUser { get; set; }
    }
}
