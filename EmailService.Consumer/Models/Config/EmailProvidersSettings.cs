using System;
using System.Collections.Generic;

namespace EmailService.Consumer.Models.Config
{
    public class EmailProvidersSettings
    {
        public List<string> SupportedProviders { get; set; }
        public int Threshold { get; set; }
        public int TimeWindowInSeconds { get; set; }
        public int DisablePeriod { get; set; }
    }
}
