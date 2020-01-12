using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace EmailService.Consumer
{
    public static class EmailProvidersStatusCheck
    {
        [FunctionName("EmailProvidersStatusCheck")]
        public static async Task Run([TimerTrigger("0 */3 * * * *")]TimerInfo myTimer, [DurableClient] IDurableEntityClient client, ILogger log)
        {
            var entityId = new EntityId(nameof(EmailProvidersStatus), "emailproviderstatus");
            await client.SignalEntityAsync<IEmailProvidersStatus>(entityId, s => s.CheckDisabledProvidersStatus());

            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
