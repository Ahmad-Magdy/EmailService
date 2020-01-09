using System;
using EmailService.Consumer.Utils;
using Xunit;

namespace EmailService.Consumer.Test
{
    public class UtilsTest
    {
        [Fact]
        public void BasicAuthenticationBase64()
        {
            string expected = "dXNlcm5hbWU6cGFzc3dvcmQ=";
            string actual = Authentication.GetBasicAuthentication("username", "password");

            Assert.StrictEqual(expected, actual);
        }
    }
}
