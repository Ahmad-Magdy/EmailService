using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;
using System;

namespace EmailService.Consumer.Services.EmailProvider
{
    public class SendGridProviderService : IEmailProvider
    {
        private SendGridClient _sendGridClient;
        ILogger<SendGridProviderService> _logger;

        public SendGridProviderService(SendGridClient sendGridClient, ILogger<SendGridProviderService> logger)
        {
            _sendGridClient = sendGridClient;
            _logger = logger;
        }

        public string ProviderName => "SendGrid";

        public async Task SendEmail(string sender, string reciver, string subject, string body)
        {
            var from = new EmailAddress(sender);
            var to = new EmailAddress(reciver);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, "", body);

            var response = await _sendGridClient.SendEmailAsync(msg);
            _logger.LogInformation($"Sent Email with response {(int)response.StatusCode}");
            if (response.StatusCode != HttpStatusCode.Accepted)
            {
                var responseBody = await response.Body.ReadAsStringAsync();
                _logger.LogError(responseBody);
                throw new Exception(responseBody);
            }

        }
    }
}
