using System;
using System.Collections.Generic;

namespace EmailService.Consumer.Utils
{
    public static class EnvironmentVariables
    {
        public static List<string> EnvironmentVariableKeyToArray(string key)
        {
            var list = new List<string>();
            int index = 0;
            while (true)
            {
                var val = Environment.GetEnvironmentVariable($"{key}__{index}");
                if (string.IsNullOrEmpty(val))
                    break;
                list.Add(val);
                index++;
            }
            return list;
        }
    }
}
