using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmailService.Consumer.Config;
using EmailService.Consumer.Models;
using EmailService.Consumer.Services.EmailProvider;
using EmailService.Consumer.Utils;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace EmailService.Consumer
{
    public interface IEmailProvidersStatus
    {
        void AddFailure(FailureRequest amount);
        Task<string> Get();
        Task CheckDisabledProvidersStatus();
        void Init();
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class EmailProvidersStatus : IEmailProvidersStatus
    {
        [JsonProperty("emailproviders")]
        public List<string> emailProviders = new List<string>();
        [JsonProperty("is_init")]
        public bool IsFirstTimeToInit = true;

        // Current rolling window of failures reported for this circuit
        [JsonProperty]
        public IDictionary<string, List<FailureRequest>> FailureWindow = new Dictionary<string, List<FailureRequest>>();

        [JsonProperty]
        public List<DisabledEmailProviderItem> disabledProviders = new List<DisabledEmailProviderItem>();
        [JsonIgnore]
        private readonly IOptions<ConfigOptions> _configOptions;
        [JsonIgnore]
        private readonly ILogger<EmailProvidersStatus> _logger;

        public EmailProvidersStatus(IOptions<ConfigOptions> configOptions, ILogger<EmailProvidersStatus> logger)
        {
            _configOptions = configOptions;
            _logger = logger;

        }

        public void Init()
        {
            if (IsFirstTimeToInit)
            {
                var _mylist = _configOptions.Value.EmailProvidersSettings.SupportedProviders;
                _mylist.ForEach(el => emailProviders.Add(el));
                IsFirstTimeToInit = false;
            }
        }

        public void AddFailure(FailureRequest req)
        {
            var state = disabledProviders.FirstOrDefault(e => e.Name == req.ProviderName);
            if (state != null)
            {
                _logger?.LogInformation($"Tried to add additional failure to {Entity.Current.EntityKey} that is already disabled.");
                return;
            }
            // Counter  because the key will be already exist in the dictionary
            if (FailureWindow.TryGetValue(req.ProviderName, out var list))
            {
                list.Add(req);
            }
            else
            {
                FailureWindow.Add(req.ProviderName, new List<FailureRequest> { req });
            }
            var timeWindow = TimeSpan.FromSeconds(_configOptions.Value.EmailProvidersSettings.TimeWindowInSeconds);
            var threshold = _configOptions.Value.EmailProvidersSettings.Threshold;


            var cutoff = req.HappenedAt.Subtract(timeWindow);

            // Filter the window only to exceptions within the cutoff timespan
            FailureWindow[req.ProviderName].RemoveAll(p => p.HappenedAt < cutoff);

            // get Items to delete or postpone

            var entityKey = Entity.Current?.EntityKey ?? "";
            if (FailureWindow[req.ProviderName].Count >= threshold)
            {
                _logger?.LogCritical($"{entityKey} Stop using email provider {req.ProviderName} because it exceeded the threshold {threshold}. Number of failure is {FailureWindow[req.ProviderName].Count}");

                emailProviders.Remove(req.ProviderName);
                var disablePeriod = _configOptions.Value.EmailProvidersSettings.DisablePeriod;
                var period = DateTimeOffset.UtcNow.Add(TimeSpan.FromSeconds(disablePeriod));
                disabledProviders.Add(new DisabledEmailProviderItem { Name = req.ProviderName, DueTime = period });
            }
            else
            {
                _logger?.LogInformation($"{entityKey} New failure occurred for provider {req.ProviderName} but it didn't reach the threshold in the timewindow yet.");
            }

        }

        public Task CheckDisabledProvidersStatus()
        {
            var now = DateTimeOffset.UtcNow;
            var itemsToEnable = disabledProviders.Where(d => d.DueTime <= now).ToList();
            foreach (var itemToEnable in itemsToEnable)
            {
                disabledProviders.Remove(itemToEnable);
                emailProviders.Add(itemToEnable.Name);
            }
            return Task.CompletedTask;
        }

        public Task<string> Get()
        {
            return Task.FromResult(emailProviders.FirstOrDefault());
        }

        [FunctionName(nameof(EmailProvidersStatus))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx)
        {
            return ctx.DispatchAsync<EmailProvidersStatus>();
        }


    }
}