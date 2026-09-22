namespace Logic.Model.Auth
{
    // Shaped around Twilio's REST API (AccountSid/AuthToken/FromNumber), since
    // that's what TwilioSmsSender talks to — swapping providers later would
    // mean a new ISmsSender implementation, not a change to this shape's callers.
    public class SmsSettings
    {
        public string AccountSid { get; set; } = string.Empty;
        public string AuthToken { get; set; } = string.Empty;
        public string FromNumber { get; set; } = string.Empty;
    }
}
