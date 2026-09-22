namespace Database.Entities
{
    // Same shape and reasoning as PasswordResetToken — a single-use, hashed,
    // expiring token rather than a permanent "confirmed" flip triggered by a
    // guessable link.
    public class EmailVerificationToken
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
