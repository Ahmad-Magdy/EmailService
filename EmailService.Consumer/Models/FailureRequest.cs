using System;
namespace EmailService.Consumer.Models
{
    public class FailureRequest
    {
        public string ProdiverName { get; set; }
        public DateTimeOffset HappenedAt { get; set; }

    }
}
