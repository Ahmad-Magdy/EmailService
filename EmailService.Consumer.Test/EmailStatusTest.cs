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
using EmailService.Consumer.Test.Helpers;
using Xunit;
using Moq;
using EmailService.Consumer.Models.Config;

namespace EmailService.Consumer.Test
{
    public class EmailStatusTest
    {
        [Fact]
        public async Task EmailStatusServiceMustKeepTheProviderIfItDidnotReachThreshold()
        {
            var emailProviderlogger = (ListLogger<FakeProviderService>)TestFactory.CreateLogger<FakeProviderService>(LoggerTypes.List);
            var functionLogger = (ListLogger<EmailService>)TestFactory.CreateLogger<EmailService>(LoggerTypes.List);
            var emailStatusLogger = (ListLogger<EmailProvidersStatus>)TestFactory.CreateLogger<EmailProvidersStatus>(LoggerTypes.List);
            var emailProvider = TestFactory.CreateFakeEmailProvider(emailProviderlogger);
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IEmailProvider>(emailProvider);

            var options = Options.Create(new ConfigOptions()
            {
                Sentry = new Config.SentryOptions(),
                EmailProviders = new EmailProviders { SendGrid = new Config.SendGrid { ApiKey = "" } },
                EmailProvidersSettings = new EmailProvidersSettings
                {
                    SupportedProviders = new List<string> { "Fake", "Fake2" },
                    Threshold = 5,
                    DisablePeriod = 10,
                    TimeWindowInSeconds = 5
                }
            });

            var durableClient = new Mock<IDurableEntityClient>();

            var emailproviderstatus = new EmailProvidersStatus(options, emailStatusLogger);
            Assert.Equal(2, emailproviderstatus.emailProviders.Count);
            durableClient.Setup(x => x.ReadEntityStateAsync<EmailProvidersStatus>(It.IsAny<EntityId>(), null, null))
                .Returns(Task.FromResult(new EntityStateResponse<EmailProvidersStatus>
                {
                    EntityExists = true,
                    EntityState = emailproviderstatus
                }));

            durableClient.Setup(x => x.SignalEntityAsync(It.IsAny<EntityId>(), "AddFailure", It.IsAny<FailureRequest>(), null, null))
                .Callback((EntityId entityId, string operation, object body, string x, string y) => {
                    var ob = body as FailureRequest;
                    emailproviderstatus.AddFailure(ob);
                })
                .Returns(Task.CompletedTask);

            var emailServiceFunction = new EmailService(emailProvider, functionLogger, serviceCollection.BuildServiceProvider(), options);

            await emailServiceFunction.RunAsync(new Models.EmailQueueItem { Sender = "me@test.dk", Receiver = "receiver@test.dk", Subject = "Subject", Body = "MyText" }, durableClient.Object);
            // Fake should be disabled 
            Assert.Single(emailproviderstatus.FailureWindow[emailproviderstatus.emailProviders.First()]);
            Assert.Empty(emailproviderstatus.disabledProviders);
            Assert.Equal(2, emailproviderstatus.emailProviders.Count);

        }
        [Fact]
        public async Task EmailStatusServiceMustDisableProviderAfterReachingThresold()
        {
            var emailProviderlogger = (ListLogger<FakeProviderService>)TestFactory.CreateLogger<FakeProviderService>(LoggerTypes.List);
            var functionLogger = (ListLogger<EmailService>)TestFactory.CreateLogger<EmailService>(LoggerTypes.List);
            var emailStatusLogger = (ListLogger<EmailProvidersStatus>)TestFactory.CreateLogger<EmailProvidersStatus>(LoggerTypes.List);
            var emailProvider = TestFactory.CreateFakeEmailProvider(emailProviderlogger);
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IEmailProvider>(emailProvider);

            var options = Options.Create(new ConfigOptions()
            {
                Sentry = new Config.SentryOptions(),
                EmailProviders = new EmailProviders { SendGrid = new Config.SendGrid { ApiKey = "" } },
                EmailProvidersSettings = new EmailProvidersSettings
                {
                    SupportedProviders = new List<string> { "Fake", "Fake2" },
                    Threshold = 1,
                    DisablePeriod = 10,
                    TimeWindowInSeconds = 5
                }
            });

            var durableClient = new Mock<IDurableEntityClient>();

            var emailproviderstatus = new EmailProvidersStatus(options, emailStatusLogger);
            Assert.Equal(2, emailproviderstatus.emailProviders.Count);
            durableClient.Setup(x => x.ReadEntityStateAsync<EmailProvidersStatus>(It.IsAny<EntityId>(), null, null))
                .Returns(Task.FromResult(new EntityStateResponse<EmailProvidersStatus>
                {
                    EntityExists = true,
                    EntityState = emailproviderstatus
                }));

            durableClient.Setup(x => x.SignalEntityAsync(It.IsAny<EntityId>(), "AddFailure",  It.IsAny<FailureRequest>(), null, null))
                .Callback((EntityId entityId, string operation, object body, string x,string y) => {
                    var ob = body as FailureRequest;
                    emailproviderstatus.AddFailure(ob);
                })
                .Returns(Task.CompletedTask);

            var emailServiceFunction = new EmailService(emailProvider, functionLogger, serviceCollection.BuildServiceProvider(), options);

            await emailServiceFunction.RunAsync(new Models.EmailQueueItem { Sender = "me@test.dk", Receiver = "receiver@test.dk", Subject = "Subject", Body = "MyText" }, durableClient.Object);
            // Fake should be disabled 
            Assert.Single(emailproviderstatus.disabledProviders);
            Assert.Equal("Fake", emailproviderstatus.disabledProviders.First().Name);
            // Fake2 Should be enabled
            Assert.Single(emailproviderstatus.emailProviders);
            Assert.Equal("Fake2", emailproviderstatus.emailProviders.First());
        }

        [Fact]
        public async Task EmailStatusServiceMustBingProviderBackAfterDisableTimePass()
        {
            var emailProviderlogger = (ListLogger<FakeProviderService>)TestFactory.CreateLogger<FakeProviderService>(LoggerTypes.List);
            var functionLogger = (ListLogger<EmailService>)TestFactory.CreateLogger<EmailService>(LoggerTypes.List);
            var emailStatusLogger = (ListLogger<EmailProvidersStatus>)TestFactory.CreateLogger<EmailProvidersStatus>(LoggerTypes.List);
            var emailProvider = TestFactory.CreateFakeEmailProvider(emailProviderlogger);
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IEmailProvider>(emailProvider);

            var options = Options.Create(new ConfigOptions()
            {
                Sentry = new Config.SentryOptions(),
                EmailProviders = new EmailProviders { SendGrid = new Config.SendGrid { ApiKey = "" } },
                EmailProvidersSettings = new EmailProvidersSettings
                {
                    SupportedProviders = new List<string> { "Fake", "Fake2" },
                    Threshold = 2,
                    DisablePeriod = 3,
                    TimeWindowInSeconds = 5
                }
            });

            var durableClient = new Mock<IDurableEntityClient>();

            var emailproviderstatus = new EmailProvidersStatus(options, emailStatusLogger);
            Assert.Equal(2, emailproviderstatus.emailProviders.Count);
            durableClient.Setup(x => x.ReadEntityStateAsync<EmailProvidersStatus>(It.IsAny<EntityId>(), null, null))
                .Returns(Task.FromResult(new EntityStateResponse<EmailProvidersStatus>
                {
                    EntityExists = true,
                    EntityState = emailproviderstatus
                }));

            durableClient.Setup(x => x.SignalEntityAsync(It.IsAny<EntityId>(), "AddFailure", It.IsAny<FailureRequest>(), null, null))
                .Callback((EntityId entityId, string operation, object body, string x, string y) => {
                    var ob = body as FailureRequest;
                    emailproviderstatus.AddFailure(ob);
                })
                .Returns(Task.CompletedTask);

            var emailServiceFunction = new EmailService(emailProvider, functionLogger, serviceCollection.BuildServiceProvider(), options);

            await emailServiceFunction.RunAsync(new Models.EmailQueueItem { Sender = "me@test.dk", Receiver = "receiver@test.dk", Subject = "Subject", Body = "MyText" }, durableClient.Object);
            await emailServiceFunction.RunAsync(new Models.EmailQueueItem { Sender = "me@test.dk", Receiver = "receiver@test.dk", Subject = "Subject", Body = "MyText" }, durableClient.Object);
            Assert.Single(emailproviderstatus.disabledProviders);
            Assert.Single(emailproviderstatus.emailProviders);
            Assert.Equal("Fake", emailproviderstatus.disabledProviders.First().Name);
            await Task.Delay(TimeSpan.FromSeconds(3));
            await emailproviderstatus.CheckDisabledProvidersStatus();
            Assert.Empty(emailproviderstatus.disabledProviders);
            Assert.Equal(2, emailproviderstatus.emailProviders.Count);

        }
    }
}
