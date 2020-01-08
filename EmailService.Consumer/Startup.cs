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

[assembly: FunctionsStartup(typeof(EmailService.Consumer.Startup))]
namespace EmailService.Consumer
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();
            var loggerConfig = new LoggerConfiguration()
                            .Enrich.FromLogContext();
             builder.Services.AddOptions<ConfigOptions>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.Bind(settings);
                });


            var humioOptions = new HumioOptions()
            {
                IngestUrl = Environment.GetEnvironmentVariable("humio__ingesturl"),
                Token = Environment.GetEnvironmentVariable("humio__token")
            };

            if (!string.IsNullOrEmpty(humioOptions?.IngestUrl) && !string.IsNullOrEmpty(humioOptions?.Token))
            {
                loggerConfig = loggerConfig.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(humioOptions.IngestUrl))
                {
                    MinimumLogEventLevel = LogEventLevel.Information,
                    ModifyConnectionSettings = x => x.BasicAuthentication(username: "", password: humioOptions.Token)
                });

            }
           
            
            if (Environment.GetEnvironmentVariable("Environment") == "Development")
            {
                loggerConfig = loggerConfig.WriteTo.Console();
            }
            Log.Logger = loggerConfig.CreateLogger();

            builder.Services.AddLogging(b =>
            {
                b.AddSerilog(dispose: true);
            });
            builder.Services.AddSingleton((serviceProvider) =>
            {
                var config = serviceProvider.GetService<IConfiguration>();
                var x = config.GetValue<string>("My");
                var api =  config.GetValue<string>("sendgrid__apikey");
                return new SendGridClient(api);
            });

            builder.Services.AddSingleton<IEmailProvider, SendGridProviderService>();
            builder.Services.AddSingleton<IEmailProvider, MailGunProviderService>();
            builder.Services.AddSingleton<IEmailProvider, AmazonSESProviderService>();
        }
    }
}
