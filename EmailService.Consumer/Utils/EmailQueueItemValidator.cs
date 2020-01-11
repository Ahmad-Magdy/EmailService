using System;
using EmailService.Consumer.Models;
using FluentValidation;

namespace EmailService.Consumer.Utils
{
    public class EmailQueueItemValidator : AbstractValidator<EmailQueueItem>
    {
        public EmailQueueItemValidator()
        {
            RuleFor(x => x.Sender).NotEmpty().EmailAddress() ;
            RuleFor(x => x.Reciver).NotEmpty().EmailAddress();
            RuleFor(x => x.Subject).NotEmpty();
            RuleFor(x => x.Body).NotEmpty().MaximumLength(100);
        }
    }
}
