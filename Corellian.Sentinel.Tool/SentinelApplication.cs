using System.Diagnostics;
using Corellian.DeathStar;
using Corellian.Sentinel.Tool.Configuration;

namespace Corellian.Sentinel.Tool
{
    public class SentinelApplication
    {
        public string Name { get; }
        public ApplicationConfiguration Configuration { get; }

        public Process? CurrentProcess { get; private set; }

        public ApplicationStatus Status { get; private set; }
        public bool AutoRestart { get; private set; }
        public int? ProcessId { get; private set; }
        public double? CpuUsage { get; private set; }
        public double? MemoryUsage { get; private set; }
        public TimeSpan? Uptime { get; private set; }

        private CpuUsageCounter? _cpuUsageCounter;
        private DateTime _lastStartTime;
        private DateTime _lastStopTime;
        private DateTime _requestedStopTime;


        public SentinelApplication(string name, ApplicationConfiguration configuration)
        {
            Name = name;
            Configuration = configuration;

            CurrentProcess = null;
            Status = ApplicationStatus.Unknown;
            AutoRestart = true;
            CpuUsage = null;
            MemoryUsage = null;
            Uptime = null;

            _cpuUsageCounter = null;
            _lastStartTime = default;
            _lastStopTime = default;
            _requestedStopTime = default;
        }

        public void TryRestart()
        {
            if (DateTime.UtcNow - _lastStartTime < TimeSpan.FromSeconds(60) ||
                DateTime.UtcNow - _lastStopTime < TimeSpan.FromSeconds(30))
            {
                Debug.WriteLine($"Waiting to start {Name}");
                return;
            }

            Start();
        }

        public void Start()
        {
            var processStartInfo = new ProcessStartInfo(Configuration.Program)
            {
                Arguments = Configuration.GetProcessArguments(),
                WorkingDirectory = Configuration.WorkingDirectory,
                CreateNoWindow = true,
                RedirectStandardInput = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false
            };

            CurrentProcess = Process.Start(processStartInfo);
            _lastStartTime = DateTime.UtcNow;
            
            if (CurrentProcess is { HasExited: false })
            {
                Status = ApplicationStatus.Starting;
                AutoRestart = true;
                _cpuUsageCounter = new CpuUsageCounter(CurrentProcess);
            }
            else
            {
                Status = ApplicationStatus.Stopped;
                AutoRestart = true;
            }
        }

        public void Stop()
        {
            if (CurrentProcess is { HasExited: false } && Status == ApplicationStatus.Running)
            {
                _ = CurrentProcess.Stop(
                    2, TimeSpan.FromSeconds(5), 10, TimeSpan.FromSeconds(1),
                    2, TimeSpan.FromSeconds(5), 10, TimeSpan.FromSeconds(1)); // TODO: use continue with and set to false?

                Status = ApplicationStatus.Stopping;
                AutoRestart = false;

                _lastStopTime = DateTime.UtcNow;
                _requestedStopTime = DateTime.UtcNow;
            }
        }

        public void Monitor()
        {
            if (CurrentProcess is { HasExited: false })
            {
                if (Status == ApplicationStatus.Stopping)
                {
                    // Ignoring as waiting for last stop result
                }
                else
                {
                    var uptime = DateTime.Now - CurrentProcess.StartTime;

                    Status = ApplicationStatus.Running;
                    ProcessId = CurrentProcess.Id;
                    if (_cpuUsageCounter.TryGetCpuUsage(out var cpuUsage))
                    {
                        CpuUsage = cpuUsage;
                    }
                    MemoryUsage = CurrentProcess.PrivateMemorySize64 / 1024.0 / 1024.0;
                    Uptime = TimeSpan.FromSeconds(Math.Truncate(uptime.TotalSeconds));
                }
            }
            else if (CurrentProcess is { HasExited: true })
            {
                CurrentProcess.Dispose();
                CurrentProcess = null;

                Status = ApplicationStatus.Stopped;
                _lastStopTime = DateTime.UtcNow;
                ProcessId = null;
                CpuUsage = null;
                MemoryUsage = null;
                Uptime = null;

                _cpuUsageCounter = null;
                _requestedStopTime = default;
            }
        }
    }
}
