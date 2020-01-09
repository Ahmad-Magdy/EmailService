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

        public async Task SendEmail(string sender, string reciver, string subject, string body)
        {
            // Do Some Magic
            await Task.Delay(TimeSpan.FromMilliseconds(200));
        }
    }
}
