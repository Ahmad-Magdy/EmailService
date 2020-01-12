using System;
using System.Collections.Generic;
using EmailService.Consumer.Models.Config;

namespace EmailService.Consumer.Config
{
    public class ConfigOptions
    {
        public EmailProviders EmailProviders { get; set; }
        public HumioOptions Humio { get; set; }
        public SentryOptions Sentry { get; set; }
        public EmailProvidersSettings EmailProvidersSettings { get; set; }
    }


}
