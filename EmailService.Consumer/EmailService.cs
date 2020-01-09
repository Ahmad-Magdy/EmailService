using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace EmailService.Consumer
{
    public class EmailService
    {
        private ILogger<EmailService> _logger;
        private List<IEmailProvider> _mylist;
        private Dictionary<string, IEmailProvider> _emailProviders = new Dictionary<string, IEmailProvider>();
        public EmailService(ILogger<EmailService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;

            _mylist = serviceProvider.GetServices<IEmailProvider>().ToList();
            _mylist.ForEach(provider => _emailProviders.Add(provider.ProviderName, provider));
        }

        [FunctionName("EmailService")]
        public async Task RunAsync([QueueTrigger("myqueue-items", Connection = "AzureWebJobsStorage")]Object myQueueItem)
        {
        }
    }
}
