using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EmailService.Consumer.Services.EmailProvider;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sentry;
using System.Linq;
using Microsoft.Extensions.Options;
using EmailService.Consumer.Config;
using EmailService.Consumer.Models;
using Newtonsoft.Json;

namespace EmailService.Consumer
{
    public class EmailService
    {
        private readonly IEmailProvider _emailProvider;
        private ILogger<EmailService> _logger;
        private readonly IOptions<ConfigOptions> _configOptions;
        private List<IEmailProvider> _mylist;
        private Dictionary<string, IEmailProvider> _emailProviders = new Dictionary<string, IEmailProvider>();
        public EmailService(IEmailProvider emailProvider, ILogger<EmailService> logger, IServiceProvider serviceProvider, IOptions<ConfigOptions> configOptions)
        {
            _emailProvider = emailProvider;
            _logger = logger;
            _configOptions = configOptions;
            _mylist = serviceProvider.GetServices<IEmailProvider>().ToList();
            _mylist.ForEach(provider => _emailProviders.Add(provider.ProviderName, provider));
        }

        [FunctionName("EmailService")]
        public async Task RunAsync([QueueTrigger("email-items", Connection = "EmailServiceStorageCS")]EmailQueueItem emailQueueItem)
        {
            using (SentrySdk.Init(_configOptions.Value.Sentry.Dsn))
            {
                try
                {
                    var currentImplementation = "SendGrid";
                    var emailService = _emailProviders[currentImplementation];
                    await _emailProvider.SendEmail(emailQueueItem.Sender, emailQueueItem.Reciver, emailQueueItem.Subject, emailQueueItem.Body);
                }
                catch (Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                    throw ex;
                }
                finally
                {
                    _logger.LogInformation($"C# Queue trigger function processed: {emailQueueItem}");
                }
            }

        }
    }
}
