using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corellian.Sentinel.Configuration.Abstractions
{
    public interface IExecutableConfigurationBuilder
    {
        string Program { get; set; }
        string WorkingDirectory { get; set; }
        List<string> ExecutableArguments { get; set; }
    }
}
