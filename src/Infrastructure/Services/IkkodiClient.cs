using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Services
{
    using Microsoft.Extensions.Options;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;

    public class IkkodiClient
    {
        public class SmsRequest
        {
            public List<string> SentTo { get; set; } = new();
            public string Message { get; set; } = string.Empty;
            public string From { get; set; } = string.Empty;
            public string SmsBroadCast { get; set; } = string.Empty;
            public string CountryStringCode { get; set; } = string.Empty;
            public string CountryNumberCode { get; set; } = string.Empty;
            public string MessageType { get; set; } = "sms";
        }
        private readonly HttpClient _httpClient;
        private readonly IkkodiSettings _settings;

        public IkkodiClient(HttpClient httpClient, IOptions<IkkodiSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;

            _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _settings.ApiKey);
        }

        public async Task SendSmsAsync(string phone, string message)
        {
            var request = new SmsRequest
            {
                SentTo = new List<string> { phone},
                Message = message,
                From = "Ikoddi",
                SmsBroadCast = "otp",
                CountryStringCode = "BF",
                CountryNumberCode = "226",
                MessageType = "sms"
            };

            var response = await _httpClient.PostAsJsonAsync("api/v1/groups/10028699/sms", request);

            response.EnsureSuccessStatusCode();
        }
    }
}
