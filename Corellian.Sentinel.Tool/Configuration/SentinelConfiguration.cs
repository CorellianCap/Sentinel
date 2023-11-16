namespace Corellian.Sentinel.Tool.Configuration
{
    public class SentinelConfiguration
    {
        public IReadOnlyDictionary<string, ApplicationConfiguration> Applications { get; set; }

        private SentinelConfiguration(IReadOnlyDictionary<string, ApplicationConfiguration> applications)
        {
            Applications = applications;
        }

        public class Builder
        {
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
                    Applications.ToDictionary(
                        a => a.Key,
                        a => a.Value.Build()));
            }
        }
    }
}
