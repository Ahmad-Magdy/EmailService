using System;
using System.Collections.Generic;

namespace EmailService.Consumer.Config
{
    public class EmailProviders
    {
        public SendGrid SendGrid { get; set; }
        public MailGun MailGun { get; set; }
        public AmazonSES AmazonSES { get; set; }
    }
    public class SendGrid
    {
        public string ApiKey { get; set; }
    }
    public class MailGun
    {
        public string ApiKey { get; set; }
        public string Domain { get; set; }
        public string BaseUrl { get; set; }
    }
    public class AmazonSES
    {
        public string KeyId { get; set; }
        public string KeySecret { get; set; }
    }
}
