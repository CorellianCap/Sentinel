namespace Corellian.Sentinel.Tool.Configuration
{
    public class SentinelConfiguration
    {
        public bool AutoRestart { get; }
        public string? BuildCommand { get; }

        public IReadOnlyDictionary<string, ApplicationConfiguration> Applications { get; }

        public SentinelConfiguration(bool autoRestart, string? buildCommand, IReadOnlyDictionary<string, ApplicationConfiguration> applications)
        {
            AutoRestart = autoRestart;
            BuildCommand = buildCommand;
            Applications = applications;
        }

        public class Builder
        {
            public bool AutoRestart { get; set; }
            public string? BuildCommand { get; set; }

            public Dictionary<string, ExecutableConfiguration.Builder> Executables { get; set; }
            public Dictionary<string, ApplicationConfiguration.Builder> Applications { get; set; }

            public Builder()
            {
                Executables = new Dictionary<string, ExecutableConfiguration.Builder>();
                Applications = new Dictionary<string, ApplicationConfiguration.Builder>();
            }

            public SentinelConfiguration Build()
            {
                return new SentinelConfiguration(
                    AutoRestart,
                    BuildCommand,
                    Applications.ToDictionary(
                        a => a.Key,
                        a => a.Value.Build()));
            }
        }
    }
}
