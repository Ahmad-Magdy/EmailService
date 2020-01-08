using System;
namespace EmailService.Consumer.Config
{
    public class ConfigOptions
    {
        public EmailProviders EmailProviders { get; set; }
        public HumioOptions Humio { get; set; }
        public SentryOptions Sentry { get; set; }
    }
}
