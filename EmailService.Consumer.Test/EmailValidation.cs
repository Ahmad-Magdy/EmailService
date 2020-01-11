using System;
using EmailService.Consumer.Models;
using EmailService.Consumer.Utils;
using Xunit;

namespace EmailService.Consumer.Test
{
    public class EmailValidation
    {
        [Fact]
        public void ValidatorMustCaptureInvalidEmail()
        {
            var emailQueueItem = new EmailQueueItem
            {
                Sender = "test",
                Reciver = "second@test.co",
                Subject = "Title",
                Body = "Body"
            };
            var validator = new EmailQueueItemValidator();
            var results = validator.Validate(emailQueueItem);

            Assert.False(results.IsValid);
            Assert.StrictEqual("Sender", results.Errors[0].PropertyName);
            Assert.StrictEqual("'Sender' is not a valid email address.", results.Errors[0].ErrorMessage);
        }
    }
}
