using System;
namespace EmailService.Consumer.Models
{
    public class FailureRequest
    {
        public string ProviderName { get; set; }
        public DateTimeOffset HappenedAt { get; set; }

    }
}
