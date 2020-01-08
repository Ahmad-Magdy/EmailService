using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Amazon;
using EmailService.Consumer.Config;

namespace EmailService.Consumer.Services.EmailProvider
{
    public class AmazonSESProvider : IEmailProvider
    {
        ILogger<AmazonSESProvider> _logger;
        private AmazonSimpleEmailServiceClient _awsSES;
        private IOptionsMonitor<ConfigOptions> _emailProvidersConfig;

        public AmazonSESProvider(IOptionsMonitor<ConfigOptions> emailProvidersConfig, ILogger<AmazonSESProvider> logger)
        {
            _logger = logger;
            // TODO: Remove redundnt method call
            _awsSES = new AmazonSimpleEmailServiceClient(emailProvidersConfig.CurrentValue.EmailProviders.AmazonSES.KeyId,
                        emailProvidersConfig.CurrentValue.EmailProviders.AmazonSES.KeySecret, RegionEndpoint.EUWest1);
        }


        public async Task SendEmail(string sender, string reciver, string subject, string body)
        {
            var sendRequest = new SendEmailRequest
            {
                Source = sender,
                Destination = new Destination
                {
                    ToAddresses =
                        new List<string> { reciver }
                },
                Message = new Message
                {
                    Subject = new Amazon.SimpleEmail.Model.Content(subject),
                    Body = new Body
                    {
                        Html = new Amazon.SimpleEmail.Model.Content
                        {
                            Charset = "UTF-8",
                            Data = body
                        }

                    }
                }
            };

            try
            {
                var response = await _awsSES.SendEmailAsync(sendRequest);
                _logger.LogInformation($"Sent Email with response {(int)response.HttpStatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Received Exception {ex.Message}");
                throw ex;
            }
        }
    }
}
