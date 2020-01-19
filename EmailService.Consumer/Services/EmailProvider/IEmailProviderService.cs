using System;
using System.Threading.Tasks;

namespace EmailService.Consumer.Services.EmailProvider
{
    public interface IEmailProvider
    {
        string ProviderName { get; }
        Task SendEmail(string sender, string receiver, string subject, string body);
    }
}
