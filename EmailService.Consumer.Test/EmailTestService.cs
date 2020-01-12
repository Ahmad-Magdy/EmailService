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
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using EmailService.Consumer.Models.Config;

namespace EmailService.Consumer.Test
{
    public class EmailTestService
    {

        [Fact]
        public async Task EmailServiceTriggerFunctionShouldLogProcessingTime()
        {
            var emailProviderlogger = (ListLogger<FakeProviderService>)TestFactory.CreateLogger<FakeProviderService>(LoggerTypes.List);
            var functionLogger = (ListLogger<EmailService>)TestFactory.CreateLogger<EmailService>(LoggerTypes.List);
            var emailProvider = TestFactory.CreateFakeEmailProvider(emailProviderlogger);
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IEmailProvider>(emailProvider);
            var durableClient = new Mock<IDurableEntityClient>();
            durableClient.Setup(x => x.ReadEntityStateAsync<EmailProvidersStatus>(It.IsAny<EntityId>(),null, null))
                .Returns(Task.FromResult(new EntityStateResponse<EmailProvidersStatus>
                {
                    EntityExists = false,
                    EntityState = null
                }));
            var options = Options.Create(new ConfigOptions() { Sentry = new SentryOptions(),
                                EmailProviders = new EmailProviders { SendGrid = new Config.SendGrid { ApiKey = "" } },
                    EmailProvidersSettings= new EmailProvidersSettings {SupportedProviders=new List<string> { "Fake" } } });

            var emailServiceFunction = new EmailService(emailProvider, functionLogger, serviceCollection.BuildServiceProvider(), options);

            await emailServiceFunction.RunAsync(new Models.EmailQueueItem { Sender = "me@test.dk", Reciver = "reciver@test.dk", Subject = "Subject", Body = "MyText" }, durableClient.Object);
            var msg = functionLogger.Logs[0];
            Assert.Contains("The request processing time was", msg);
        }

        [Fact]
        public async Task EmailServiceTriggerFunctionMustNotProcessEmptyQueueItem()
        {
            var emailProviderlogger = (ListLogger<FakeProviderService>)TestFactory.CreateLogger<FakeProviderService>(LoggerTypes.List);
            var functionLogger = (ListLogger<EmailService>)TestFactory.CreateLogger<EmailService>(LoggerTypes.List);
            var emailProvider = TestFactory.CreateFakeEmailProvider(emailProviderlogger);

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IEmailProvider>(emailProvider);

            var durableClient = new Mock<IDurableEntityClient>();
            durableClient.Setup(x => x.ReadEntityStateAsync<EmailProvidersStatus>(It.IsAny<EntityId>(), null, null))
                .Returns(Task.FromResult(new EntityStateResponse<EmailProvidersStatus>
                {
                    EntityExists = false,
                    EntityState = null
                }));

            var options = Options.Create(new ConfigOptions()
            {
                Sentry = new SentryOptions(),
                EmailProviders = new EmailProviders { SendGrid = new Config.SendGrid { ApiKey = "" } },
                EmailProvidersSettings = new EmailProvidersSettings { SupportedProviders = new List<string> { "Fake" } }
            });

            var emailServiceFunction = new EmailService(emailProvider, functionLogger, serviceCollection.BuildServiceProvider(), options);

            await emailServiceFunction.RunAsync(new Models.EmailQueueItem { }, durableClient.Object);

            var msg = functionLogger.Logs[0];
            Assert.Contains("'Sender' must not be empty", msg);
            Assert.Contains("'Reciver' must not be empty.", msg);
            Assert.Contains("'Subject' must not be empty", msg);
            Assert.Contains("'Body' must not be empty", msg);
        }

    }
}
