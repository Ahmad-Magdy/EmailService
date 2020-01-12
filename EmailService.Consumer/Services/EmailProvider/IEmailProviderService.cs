using System;
using System.Threading.Tasks;

namespace EmailService.Consumer.Services.EmailProvider
{
    public interface IEmailProvider
    {
        string ProviderName { get; }
        Task SendEmail(string sender, string reciver, string subject, string body);
    }
}
