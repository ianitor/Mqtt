using Ianitor.Common.CommandLineParser;
using Microsoft.Extensions.Options;
using NLog;

namespace MqttBenchmark.Commands
{
    internal abstract class MqttBenchCommand : IMqttBenchCommand
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly string _commandDescription;

        protected MqttBenchCommand(string commandValue, string commandDescription, IOptions<MqttBenchOptions> options)
        {
            _commandDescription = commandDescription;
            CommandValue = commandValue;
            Options = options;
        }

        public string CommandValue { get; }

        protected ICommandArgumentValue CommandArgumentValue { get; private set; }
        protected IOptions<MqttBenchOptions> Options { get; }

        public void AddCommand(ICommandArgument commandArgument)
        {
            CommandArgumentValue = commandArgument.AddCommandValue(CommandValue, _commandDescription);
            AddArguments();
        }

        protected virtual void AddArguments()
        {

        }

        public virtual Task PreValidate()
        {
            return Task.CompletedTask;
        }

        public abstract Task Execute();


    }
}
