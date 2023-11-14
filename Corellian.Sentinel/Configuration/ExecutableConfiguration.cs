using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Corellian.Sentinel.Configuration.Abstractions;

namespace Corellian.Sentinel.Configuration
{
    public class ExecutableConfiguration
    {
        public string Program { get; }
        public string WorkingDirectory { get; }
        public IReadOnlyList<string> ExecutableArguments { get; }

        private ExecutableConfiguration(string program, string workingDirectory, IReadOnlyList<string> executableArguments)
        {
            Program = program;
            WorkingDirectory = workingDirectory;
            ExecutableArguments = executableArguments;
        }

        public class Builder : IExecutableConfigurationBuilder
        {
            public string Program { get; set; }
            public string WorkingDirectory { get; set; }
            public List<string> ExecutableArguments { get; set; }

            public Builder()
            {
                ExecutableArguments = new List<string>();
            }

            public ExecutableConfiguration Build()
            {
                return new ExecutableConfiguration(
                    Program,
                    WorkingDirectory,
                    ExecutableArguments);
            }
        }
    }
}
