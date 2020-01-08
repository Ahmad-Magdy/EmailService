using System;
using System.Text;

namespace EmailService.Consumer.Utils
{
    public static class Authentication
    {
        public static string GetBasicAuthentication(string username, string password) => Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
    }
}
