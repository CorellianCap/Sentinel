namespace Corellian.Sentinel.Tool.Configuration.Abstractions
{
    public interface IExecutableConfigurationBuilder
    {
        string Program { get; set; }
        string WorkingDirectory { get; set; }
        List<string> ExecutableArguments { get; set; }
    }
}
