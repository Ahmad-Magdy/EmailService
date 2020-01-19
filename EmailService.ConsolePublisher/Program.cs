using System;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Newtonsoft.Json;

namespace EmailService.ConsolePublisher
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var emailConnetionString = Environment.GetEnvironmentVariable("EmailServiceStorageCS");
            var queueName = Environment.GetEnvironmentVariable("QueueName");
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(emailConnetionString);

            var queueClient = storageAccount.CreateCloudQueueClient();
            var queue = queueClient.GetQueueReference(queueName);
            await queue.CreateIfNotExistsAsync();
            var count = 0;
            while(count <= 5000)
            {
                var emailItem = new EmailQueueItem
                {
                    Sender = "Me@me.com",
                    Receiver = "m2@m.com",
                    Subject = "Subject 1",
                    Body = "Test"
                };
                var emailQueueItem = new CloudQueueMessage(JsonConvert.SerializeObject(emailItem));
                await queue.AddMessageAsync(emailQueueItem);
                Console.WriteLine("Message Sent");
                count++;
            }
        }
    }

    public class EmailQueueItem
    {
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
