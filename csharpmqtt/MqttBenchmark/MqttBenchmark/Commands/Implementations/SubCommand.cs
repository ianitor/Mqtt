using System.Net;
using Ianitor.Common.CommandLineParser;
using Microsoft.Extensions.Options;

namespace MqttBenchmark.Commands.Implementations
{
    internal class SubCommand : MqttBenchCommand
    {
        private IArgument _serverAddress;
        private IArgument _clientCount;

        public SubCommand(IOptions<MqttBenchOptions> options, IBenchMqttClient benchMqttClient)
            : base("Sub", "Subscribe to messages of a broker", options)
        {
        }

        protected override void AddArguments()
        {
            _serverAddress = CommandArgumentValue.AddArgument("s", "server", new[] { "DNS or IP of MQTT broker" },
                true, 1);
            _clientCount = CommandArgumentValue.AddArgument("c", "clientCount", new[] { "Number of clients" },
                true, 1);
        }

        public override async Task Execute()
        {
            var serverAddressArg = CommandArgumentValue.GetArgumentValue(_serverAddress);
            var address = serverAddressArg.GetValue<string>();

            var clientCountArg = CommandArgumentValue.GetArgumentValue(_clientCount);
            var clientCount = clientCountArg.GetValue<int>();
            //
            // var clientNameArgData = CommandArgumentValue.GetArgumentValue(_clientName);
            // var clientName = clientNameArgData.GetValue<string>();

            Logger.Info($"Connecting broker '{address}'");

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            Console.CancelKeyPress += (sender, args) => cancellationTokenSource.Cancel();

            var clientTaskList = new List<Task>();

            string hostName = Dns.GetHostName(); 

            for (int i = 0; i < clientCount; i++)
            {
                var clientId = $"client_{i}";

                clientTaskList.Add(Task.Run(async () =>
                    {
                        using (var benchMqttClient = new BenchMqttClient())
                        {
                            Logger.Info($"Connecting broker '{address}' as client '{clientId}_sub'");

                            await benchMqttClient.Connect(address, clientId + "_sub");
                            await benchMqttClient.Subscribe($"samples/{clientId}/#");

                            Logger.Info($"Client '{clientId}' subscription started.");

                            while (!cancellationTokenSource.IsCancellationRequested)
                            {
                                Thread.Sleep(1000);
                                
                                Logger.Info($"'{clientId}': Received '{benchMqttClient.MessagesReceivedCount}' messages");
                            }
                        }
                    }, cancellationTokenSource.Token)
                );
            }

            Logger.Info($"Clients created. Press CTRL-C for stop");


            Task.WaitAll(clientTaskList.ToArray(), TimeSpan.FromDays(1));


            Logger.Info($"Sub done");
            PerfManager.Instance.WriteLogging(Logger);
        }
    }
}