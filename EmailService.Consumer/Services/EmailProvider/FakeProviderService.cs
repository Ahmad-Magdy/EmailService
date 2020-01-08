using System;
using System.Threading.Tasks;

namespace EmailService.Consumer.Services.EmailProvider
{
    public class FakeProviderService : IEmailProvider
    {
        public FakeProviderService()
        {
        }

        public string ProviderName => "Fake";

        public async Task SendEmail(string sender, string reciver, string subject, string body)
        {
            // Do Some Magic
            await Task.Delay(TimeSpan.FromMilliseconds(200));
        }
    }
}
