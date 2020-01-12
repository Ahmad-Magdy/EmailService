using System;
namespace EmailService.Consumer.Models
{
    public class DisabledEmailProviderItem
    {
        public string Name { get; set; }
        public DateTimeOffset DueTime { get; set; }
    }
}
