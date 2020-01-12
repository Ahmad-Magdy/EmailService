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
using System.Diagnostics;
using EmailService.Consumer.Utils;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

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
        public async Task RunAsync([QueueTrigger("email-items", Connection = "EmailServiceStorageCS")]EmailQueueItem emailQueueItem,
            [DurableClient] IDurableEntityClient client)
        {
            var entityId = new EntityId(nameof(EmailProvidersStatus), "emailproviderstatus");
            var entity = await client.ReadEntityStateAsync<EmailProvidersStatus>(entityId);
            string providerToUse;
            // Entity doesn't exist yet, take the first supported provider
            if (!entity.EntityExists)
                providerToUse = _configOptions.Value.EmailProvidersSettings.SupportedProviders.FirstOrDefault();
            else
                providerToUse = await entity.EntityState.Get();

            if (string.IsNullOrEmpty(providerToUse))
                throw new Exception("No Email Providers are found, maybe all providers are disabled because there are unhealthy!. Using retries..");

            var stopWatch = Stopwatch.StartNew();
            using (SentrySdk.Init(_configOptions.Value.Sentry.Dsn))
            {
                try
                {
                    var validator = new EmailQueueItemValidator();
                    var validationResults = await validator.ValidateAsync(emailQueueItem);
                    if (!validationResults.IsValid)
                    {
                        var errorMessage = validationResults.Errors
                                                            .Select(ve => $"{ve.PropertyName} {ve.ErrorMessage}")
                                                            .Aggregate("", (acc, curr) => $"{acc}\n{curr}");
                        _logger.LogError(errorMessage);
                        _logger.LogError("The provided queue item is invalid and will be skipped");
                    }
                    else
                    {
                        var isSuccessful = await _emailProviders[providerToUse].SendEmail(emailQueueItem.Sender, emailQueueItem.Reciver, emailQueueItem.Subject, emailQueueItem.Body);
                        if (!isSuccessful)
                            await client.SignalEntityAsync<IEmailProvidersStatus>(entityId, e =>
                                                e.AddFailure(new FailureRequest { ProdiverName = providerToUse, HappenedAt = DateTimeOffset.UtcNow }));

                    }

                }
                catch (Exception ex)
                {
                    // Capture the exception and send it to Sentry, then rethrow it to retry executing it.
                    SentrySdk.CaptureException(ex);
                    await client.SignalEntityAsync<IEmailProvidersStatus>(entityId, e =>
                    e.AddFailure(new FailureRequest { ProdiverName = providerToUse, HappenedAt = DateTimeOffset.UtcNow }));
                    throw ex;
                }
                finally
                {
                    stopWatch.Stop();
                    _logger.LogInformation($"The request processing time was {stopWatch.ElapsedMilliseconds}");
                    _logger.LogInformation($"C# Queue trigger function processed: {JsonConvert.SerializeObject(emailQueueItem)}");
                }
            }

        }
    }
}
