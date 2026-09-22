namespace Logic.Model.Auth
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserInfo User { get; set; } = new UserInfo();
    }

    public class UserInfo
    {
        public long Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public long RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public long? StationId { get; set; }
        public bool EmailVerified { get; set; }
    }
}
