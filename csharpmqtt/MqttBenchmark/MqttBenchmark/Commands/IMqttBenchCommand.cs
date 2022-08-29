using Ianitor.Common.CommandLineParser;

namespace MqttBenchmark.Commands
{
    public interface IMqttBenchCommand
    {
        string CommandValue { get; }
        void AddCommand(ICommandArgument commandArgument);

        Task PreValidate();

        Task Execute();
    }
}
