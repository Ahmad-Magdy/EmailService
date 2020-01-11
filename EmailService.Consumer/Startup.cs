using System;
using System.Linq;
using EmailService.Consumer.Config;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using SendGrid;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using EmailService.Consumer.Services.EmailProvider;
using EmailService.Consumer.Extensions;
using Microsoft.Extensions.Options;

[assembly: FunctionsStartup(typeof(EmailService.Consumer.Startup))]
namespace EmailService.Consumer
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();
            builder.Services.AddOptions<ConfigOptions>()
                            .Configure<IConfiguration>((settings, configuration) =>
                            {
                                configuration.Bind(settings);
                            });


            builder.Services.AddSerilogLogger();

            builder.Services.AddSingleton((serviceProvider) =>
            {
                var config = serviceProvider.GetService<IOptions<ConfigOptions>>();
                var apiKey = config.Value.EmailProviders.SendGrid.ApiKey;
                return new SendGridClient(apiKey);
            });

            builder.Services.AddSingleton<IEmailProvider, SendGridProviderService>();
            builder.Services.AddSingleton<IEmailProvider, MailGunProviderService>();
            builder.Services.AddSingleton<IEmailProvider, AmazonSESProviderService>();
            builder.Services.AddSingleton<IEmailProvider, FakeProviderService>();
        }
    }
}
