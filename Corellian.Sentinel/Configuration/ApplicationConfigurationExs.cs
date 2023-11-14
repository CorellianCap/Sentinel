using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corellian.Sentinel.Configuration
{
    public static class ApplicationConfigurationExs
    {
        public static string? GetProcessArguments(this ApplicationConfiguration configuration)
        {
            if (!configuration.Arguments.Any())
            {
                return null;
            }

            var processArguments = string.Empty;

            foreach (string arg in configuration.Arguments)
            {
                if (arg.Contains(' '))
                {
                    processArguments += "\"" + arg + "\" ";
                }
                else
                {
                    processArguments += arg + " ";
                }
            }

            return processArguments.Remove(processArguments.Length - 1);
        }
    }
}
