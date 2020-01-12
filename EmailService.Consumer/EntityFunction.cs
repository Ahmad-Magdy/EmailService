using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmailService.Consumer.Config;
using EmailService.Consumer.Services.EmailProvider;
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
    }

    public class EmailProviderStatusModel
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }

    }
    public class FailureRequest
    {
        public string ProdiverName { get; set; }
        public DateTimeOffset At { get; set; }

    }
    public class DisabledEmailProviderItem
    {
        public string Name { get; set; }
        public DateTimeOffset DueTime { get; set; }
    }
    [JsonObject(MemberSerialization.OptIn)]
    public class EmailProvidersStatus : IEmailProvidersStatus
    {
        [JsonProperty("value")]
        public List<EmailProviderStatusModel> emailProviders;

        // Current rolling window of failures reported for this circuit
        [JsonProperty]
        public IDictionary<string, List<FailureRequest>> FailureWindow = new Dictionary<string, List<FailureRequest>>();

        [JsonProperty]
        public List<DisabledEmailProviderItem> disabledProviders = new List<DisabledEmailProviderItem>();

        private readonly IOptions<ConfigOptions> _configOptions;
        private readonly ILogger<EmailProvidersStatus> _logger;

        public EmailProvidersStatus(IOptions<ConfigOptions> configOptions, ILogger<EmailProvidersStatus> logger)
        {
            _configOptions = configOptions;
            _logger = logger;
            var _mylist = _configOptions.Value.EmailProvidersSettings.SupportedProviders;
            emailProviders = _mylist.Select(ep => new EmailProviderStatusModel { Name = ep, IsActive = true }).ToList();
        }

        public void AddFailure(FailureRequest req)
        {
            var state = disabledProviders.FirstOrDefault(e => e.Name == req.ProdiverName);
            if (state != null)
            {
                _logger.LogInformation($"Tried to add additional failure to {Entity.Current.EntityKey} that is already open.");
                return;
            }
            // Counter  because the key will be already exist in the dictionary
            if (FailureWindow.TryGetValue(req.ProdiverName, out var list))
            {
                list.Add(req);
            }
            else
            {
                FailureWindow.Add(req.ProdiverName, new List<FailureRequest> { req });
            }
            var timeWindow = TimeSpan.FromSeconds(_configOptions.Value.EmailProvidersSettings.TimeWindowInSeconds);
            var threshold = _configOptions.Value.EmailProvidersSettings.Threshold;


            var cutoff = req.At.Subtract(timeWindow);

            // Filter the window only to exceptions within the cutoff timespan
            FailureWindow[req.ProdiverName].RemoveAll(p => p.At < cutoff);

            // get Items to delete or postpone


            if (FailureWindow[req.ProdiverName].Count >= threshold)
            {
                _logger.LogCritical($"{Entity.Current.EntityKey} Stop using email provider {req.ProdiverName} because it exceeded the threshold {threshold}. Number of failure is {FailureWindow[req.ProdiverName].Count}");

                var emailProviderItem = emailProviders.FirstOrDefault(e => e.Name == req.ProdiverName);
                // Disable and move to the end of the list
                emailProviderItem.IsActive = false;
                emailProviders.Remove(emailProviderItem);
                var disablePeriod = _configOptions.Value.EmailProvidersSettings.DisablePeriod;
                var period = DateTimeOffset.UtcNow.Add(TimeSpan.FromSeconds(disablePeriod));
                disabledProviders.Add(new DisabledEmailProviderItem { Name = emailProviderItem.Name, DueTime = period });
            }
            else
            {
                _logger.LogInformation($"{Entity.Current.EntityKey} New failure acconted for provider {req.ProdiverName} but it didn't reach the threshold in the timewindow yet.");
            }

        }

        public Task CheckDisabledProvidersStatus()
        {
            var now = DateTimeOffset.UtcNow;
            var itemsToEnable = disabledProviders.Where(d => d.DueTime <= now);
            foreach (var itemToEnable in itemsToEnable)
            {
                disabledProviders.Remove(itemToEnable);
                emailProviders.Add(new EmailProviderStatusModel { Name = itemToEnable.Name, IsActive = true });
            }
            return Task.CompletedTask;
        }

        public Task<string> Get()
        {
            return Task.FromResult(emailProviders.FirstOrDefault()?.Name);
        }

        [FunctionName(nameof(EmailProvidersStatus))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx)
        {
            return ctx.DispatchAsync<EmailProvidersStatus>();
        }


    }
}