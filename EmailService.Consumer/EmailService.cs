using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace EmailService.Consumer
{
    public class EmailService
    {
        private ILogger<EmailService> _logger;

        public EmailService( ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        [FunctionName("EmailService")]
        public void Run([QueueTrigger("myqueue-items", Connection = "AzureWebJobsStorage")]string myQueueItem)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
        }
    }
}
