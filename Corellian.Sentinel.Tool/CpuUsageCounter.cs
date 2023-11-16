using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corellian.Sentinel
{
    public class CpuUsageCounter
    {
        private readonly Process _process;

        private DateTime _lastTime;
        private TimeSpan _lastTotalProcessorTime;

        public CpuUsageCounter(Process process)
        {
            _process = process;

            _lastTime = default;
            _lastTotalProcessorTime = default;
        }

        public bool TryGetCpuUsage(out double cpuUsage)
        {
            var currentTime = DateTime.Now;
            var currentTotalProcessorTime = _process.TotalProcessorTime;

            if (_lastTime == default || _lastTotalProcessorTime == default)
            {
                _lastTime = currentTime;
                _lastTotalProcessorTime = currentTotalProcessorTime;

                cpuUsage = default;
                return false;
            }
            else
            {
                cpuUsage = (currentTotalProcessorTime - _lastTotalProcessorTime).TotalMilliseconds / 
                           (currentTime - _lastTime).TotalMilliseconds / Convert.ToDouble(Environment.ProcessorCount);

                _lastTime = currentTime;
                _lastTotalProcessorTime = currentTotalProcessorTime;

                return true;
            }
        }
    }
}
