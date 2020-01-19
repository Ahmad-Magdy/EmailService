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
    public class AmazonSESProviderService : IEmailProvider
    {
        ILogger<AmazonSESProviderService> _logger;
        private AmazonSimpleEmailServiceClient _awsSES;

        public AmazonSESProviderService(IOptions<ConfigOptions> emailProvidersConfig, ILogger<AmazonSESProviderService> logger)
        {
            _logger = logger;
            // TODO: Remove redundant method call
            _awsSES = new AmazonSimpleEmailServiceClient(emailProvidersConfig.Value.EmailProviders.AmazonSES.KeyId,
                        emailProvidersConfig.Value.EmailProviders.AmazonSES.KeySecret, RegionEndpoint.EUWest1);
        }

        public string ProviderName => "AWSSES";

        public async Task SendEmail(string sender, string receiver, string subject, string body)
        {
            var sendRequest = new SendEmailRequest
            {
                Source = sender,
                Destination = new Destination
                {
                    ToAddresses =
                        new List<string> { receiver }
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
