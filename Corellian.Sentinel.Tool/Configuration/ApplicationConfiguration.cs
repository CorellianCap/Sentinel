using Corellian.Sentinel.Configuration.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corellian.Sentinel.Configuration
{
    public class ApplicationConfiguration
    {
        public string Program { get; }
        public string WorkingDirectory { get; }
        public IReadOnlyList<string> Arguments { get; }

        private ApplicationConfiguration(string program, string workingDirectory, IReadOnlyList<string> arguments)
        {
            Program = program;
            WorkingDirectory = workingDirectory;
            Arguments = arguments;
        }

        public class Builder : IExecutableConfigurationBuilder
        {
            public string Program { get; set; }
            public string WorkingDirectory { get; set; }
            public List<string> ExecutableArguments { get; set; }
            public List<string> ApplicationArguments { get; set; }

            public Builder()
            {
                ExecutableArguments = new List<string>();
                ApplicationArguments = new List<string>();
            }

            public ApplicationConfiguration Build()
            {
                return new ApplicationConfiguration(
                    Program,
                    WorkingDirectory,
                    ExecutableArguments.Concat(ApplicationArguments).Distinct().ToList());
            }
        }
    }
}
