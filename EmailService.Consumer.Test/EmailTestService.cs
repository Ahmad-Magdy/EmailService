using System;
using EmailService.Consumer.Test.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using EmailService.Consumer.Services.EmailProvider;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using EmailService.Consumer.Config;

namespace EmailService.Consumer.Test
{
    public class EmailTestService
    {

        [Fact]
        public async void EmailServiceTriggerFunctionShouldLogProcessingTime()
        {
            var emailProviderlogger = (ListLogger<FakeProviderService>)TestFactory.CreateLogger<FakeProviderService>(LoggerTypes.List);
            var functionLogger = (ListLogger<EmailService>)TestFactory.CreateLogger<EmailService>(LoggerTypes.List);
            var emailProvider = TestFactory.CreateFakeEmailProvider(emailProviderlogger);
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IEmailProvider>(emailProvider);

            var options = Options.Create(new ConfigOptions() { Sentry = new SentryOptions(), EmailProviders = new EmailProviders { SendGrid = new Config.SendGrid { ApiKey = "" } } });

            var e = new EmailService(emailProvider, functionLogger, serviceCollection.BuildServiceProvider(), options);

            await e.RunAsync(new Models.EmailQueueItem { Sender = "me@test.dk", Reciver = "reciver@test.dk", Subject = "Subject", Body = "MyText" });
            var msg = functionLogger.Logs[0];
            Assert.Contains("The request processing time was", msg);
        }

        [Fact]
        public async void EmailServiceTriggerFunctionMustNotProcessEmptyQueueItem()
        {
            var emailProviderlogger = (ListLogger<FakeProviderService>)TestFactory.CreateLogger<FakeProviderService>(LoggerTypes.List);
            var functionLogger = (ListLogger<EmailService>)TestFactory.CreateLogger<EmailService>(LoggerTypes.List);
            var emailProvider = TestFactory.CreateFakeEmailProvider(emailProviderlogger);
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IEmailProvider>(emailProvider);

            var options = Options.Create(new ConfigOptions() { Sentry = new SentryOptions(), EmailProviders = new EmailProviders { SendGrid = new Config.SendGrid { ApiKey = "" } } });

            var e = new EmailService(emailProvider, functionLogger, serviceCollection.BuildServiceProvider(), options);

            await e.RunAsync(new Models.EmailQueueItem { });

            var msg = functionLogger.Logs[0];
            Assert.Contains("'Sender' must not be empty", msg);
            Assert.Contains("'Reciver' must not be empty.", msg);
            Assert.Contains("'Subject' must not be empty", msg);
            Assert.Contains("'Body' must not be empty", msg);
        }
    }
}
