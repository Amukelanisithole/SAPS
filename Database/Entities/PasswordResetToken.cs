namespace Database.Entities
{
    public class PasswordResetToken
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        // SHA-256 hash of the token that's emailed to the user — the raw value
        // is never stored, so a database read alone can't be used to reset a
        // password (same reasoning as storing a bcrypt hash instead of a
        // plaintext password).
        public string TokenHash { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool Used { get; set; }
        public DateTime CreatedDate { get; set; }

        public User? NavUser { get; set; }
    }
}
