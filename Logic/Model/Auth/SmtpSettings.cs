namespace Logic.Model.Auth
{
    public class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromAddress { get; set; } = "no-reply@saps.local";
        public string FromName { get; set; } = "SAPS Crime Pre-Reporting";
    }
}
