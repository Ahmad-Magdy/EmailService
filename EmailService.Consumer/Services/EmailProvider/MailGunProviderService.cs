using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Net;
using EmailService.Consumer.Config;
using EmailService.Consumer.Utils;

namespace EmailService.Consumer.Services.EmailProvider
{
    public class MailGunProviderService : IEmailProvider
    {
        ILogger<MailGunProviderService> _logger;
        private HttpClient _httpClient;
        private IOptionsMonitor<ConfigOptions> _emailProvidersConfig;

        public MailGunProviderService(IHttpClientFactory httpClientFactory, IOptionsMonitor<ConfigOptions> emailProvidersConfig, ILogger<MailGunProviderService> logger)
        {
            _emailProvidersConfig = emailProvidersConfig;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(emailProvidersConfig.CurrentValue.EmailProviders.MailGun.BaseUrl);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                                                Authentication.GetBasicAuthentication("api", _emailProvidersConfig.CurrentValue.EmailProviders.MailGun.ApiKey));
            _logger = logger;
        }

        public async Task SendEmail(string sender, string reciver, string subject, string body)
        {
            var domain = _emailProvidersConfig.CurrentValue.EmailProviders.MailGun.Domain;
            var emailDetails = new Dictionary<string, string> {
                {"from", $"{sender}" },
                {"to", reciver},
                {"subject", subject},
                {"html",body }
            };
            var response = await _httpClient.PostAsync($"v3/{domain}/messages", new FormUrlEncodedContent(emailDetails));
            _logger.LogInformation($"Sent Email with response {(int)response.StatusCode}");
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogError(responseBody);
            }
        }
    }

}
