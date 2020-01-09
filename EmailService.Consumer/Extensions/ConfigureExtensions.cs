using System;
using EmailService.Consumer.Config;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

namespace EmailService.Consumer.Extensions
{
    public static class ConfigureExtensions
    {
        public static void AddSerilogLogger(this IServiceCollection serviceCollection)
        {
            var loggerConfig = new LoggerConfiguration()
                .Enrich.FromLogContext();
            var humioOptions = new HumioOptions()
            {
                IngestUrl = Environment.GetEnvironmentVariable("humio__ingesturl"),
                Token = Environment.GetEnvironmentVariable("humio__token")
            };

            if (!string.IsNullOrEmpty(humioOptions?.IngestUrl) && !string.IsNullOrEmpty(humioOptions?.Token))
            {
                loggerConfig = loggerConfig.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(humioOptions.IngestUrl))
                {
                    FailureCallback = e => Console.WriteLine("Unable to submit event " + e.MessageTemplate),
                    EmitEventFailure = EmitEventFailureHandling.RaiseCallback,
                    MinimumLogEventLevel = LogEventLevel.Information,
                    ModifyConnectionSettings = x => x.BasicAuthentication(username: "", password: humioOptions.Token)
                });
            }
            var sentryDsn = Environment.GetEnvironmentVariable("sentry__dsn");
            if (!string.IsNullOrEmpty(sentryDsn))
            {
                loggerConfig = loggerConfig.WriteTo.Sentry(sentryDsn);
            }

            if (Environment.GetEnvironmentVariable("Environment") == "Development")
            {
                loggerConfig = loggerConfig.WriteTo.Console();
            }
            Log.Logger = loggerConfig.CreateLogger();

            serviceCollection.AddLogging(b =>
            {
                b.AddSerilog(dispose: true);
            });

        }


    }
}
