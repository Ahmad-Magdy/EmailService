using System;
namespace EmailService.Consumer.Models
{
    public class EmailQueueItem
    {
        public string Sender { get; set; }
        public string Reciver { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
