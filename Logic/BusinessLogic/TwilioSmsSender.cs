using System.Net.Http.Headers;
using System.Text;
using Logic.BusinessLogicInterface;
using Logic.Model.Auth;
using Microsoft.Extensions.Options;

namespace Logic.BusinessLogic
{
    public class TwilioSmsSender : ISmsSender
    {
        private readonly HttpClient _httpClient;
        private readonly SmsSettings _settings;

        public TwilioSmsSender(HttpClient httpClient, IOptions<SmsSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public async Task SendAsync(string toPhoneNumber, string message, CancellationToken ct = default)
        {
            var url = $"https://api.twilio.com/2010-04-01/Accounts/{_settings.AccountSid}/Messages.json";

            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_settings.AccountSid}:{_settings.AuthToken}"));

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["To"] = toPhoneNumber,
                    ["From"] = _settings.FromNumber,
                    ["Body"] = message
                })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            var response = await _httpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();
        }
    }
}
