using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace EmailService.Consumer.Services.EmailProvider
{
    public class FakeProviderService : IEmailProvider
    {
        private readonly ILogger<FakeProviderService> _logger;

        public FakeProviderService(ILogger<FakeProviderService> logger)
        {
            _logger = logger;
        }

        public string ProviderName => "Fake";

        public async Task SendEmail(string sender, string receiver, string subject, string body)
        {
            // Do Some Magic
            _logger.LogInformation("Calling SendEmail from Fake email service ");
            await Task.Delay(TimeSpan.FromMilliseconds(200));
            throw new Exception($"The service {ProviderName} is not working in the meantime.");
        }
    }
}
