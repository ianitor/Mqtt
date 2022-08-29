using Microsoft.Extensions.Options;

namespace MqttBenchmark
{
    internal interface IParser
    {
        IOptions<MqttBenchOptions> Options { get; }
        void ShowUsageInformation();
        Task ParseAndValidateAsync();
        void CreateSamples();
    }
}