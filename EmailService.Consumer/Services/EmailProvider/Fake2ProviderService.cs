using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace EmailService.Consumer.Services.EmailProvider
{
    public class Fake2ProviderService : IEmailProvider
    {
        private readonly ILogger<FakeProviderService> _logger;

        public Fake2ProviderService(ILogger<FakeProviderService> logger)
        {
            _logger = logger;
        }

        public string ProviderName => "Fake2";

        public async Task<bool> SendEmail(string sender, string reciver, string subject, string body)
        {
            // Do Some Magic
            _logger.LogInformation("Calling SendEmail from Fake email service2 ");
            await Task.Delay(TimeSpan.FromMilliseconds(200));
            return true;
        }
    }
}
