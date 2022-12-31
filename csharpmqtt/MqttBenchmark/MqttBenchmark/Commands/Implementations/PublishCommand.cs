using System.Net;
using Ianitor.Common.CommandLineParser;
using Microsoft.Extensions.Options;

namespace MqttBenchmark.Commands.Implementations
{
    internal class PublishCommand : MqttBenchCommand
    {
        private IArgument _serverAddress;
        private IArgument _clientCount;
        private IArgument _messagesByClient;
        private IArgument _messageInterval;

        public PublishCommand(IOptions<MqttBenchOptions> options, IBenchMqttClient benchMqttClient)
            : base("Pub", "Publish messages to a broker", options)
        {
        }

        protected override void AddArguments()
        {
            _serverAddress = CommandArgumentValue.AddArgument("s", "server", new[] { "DNS or IP of MQTT broker" },
                true, 1);
            _clientCount = CommandArgumentValue.AddArgument("c", "clientCount", new[] { "Number of clients" },
                true, 1);
            _messagesByClient = CommandArgumentValue.AddArgument("mc", "messagesByClient", new[] { "Number of messages sent for each client" },
                true, 1);
            _messageInterval = CommandArgumentValue.AddArgument("i", "interval", new[] { "Interval of sending messages by client (e. g. 100ms to simulate sensors)" },
                false, 1);
            
        }

        public override async Task Execute()
        {
            var serverAddressArg = CommandArgumentValue.GetArgumentValue(_serverAddress);
            var address = serverAddressArg.GetValue<string>();
            
            var clientCountArg = CommandArgumentValue.GetArgumentValue(_clientCount);
            var clientCount = clientCountArg.GetValue<int>();
            
            var messagesByClientArg = CommandArgumentValue.GetArgumentValue(_messagesByClient);
            var messageByClientCount = messagesByClientArg.GetValue<int>();

            int? messageIntervalTimeMs = null;
            if (CommandArgumentValue.IsArgumentUsed(_messageInterval))
            {
                var messageIntervalArg = CommandArgumentValue.GetArgumentValue(_messageInterval);
                messageIntervalTimeMs = messageIntervalArg.GetValue<int>();
            }
    

            Logger.Info($"Connecting broker '{address}'");

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            
            Console.CancelKeyPress += (sender, args) => cancellationTokenSource.Cancel();

            var clientTaskList = new List<Task>();

            long messageCount = 0;
            decimal maxMessageCount = clientCount * messageByClientCount;
            long reportCount = clientCount * messageByClientCount / 100;
            if (reportCount == 0)
            {
                reportCount = 1;
            }
            
            string hostName = Dns.GetHostName(); 

            for (int i = 0; i < clientCount; i++)
            {
                var clientId = $"client_{i}";

                clientTaskList.Add(Task.Run(async () =>
                    {
                        using (var benchMqttClient = new BenchMqttClient())
                        {
                            Logger.Info($"Connecting broker '{address}' as client '{clientId}'");

                            await benchMqttClient.Connect(address, clientId);

                            for (int j = 0; j < messageByClientCount; j++)
                            {
                                if (messageIntervalTimeMs.HasValue)
                                {
                                    Thread.Sleep(messageIntervalTimeMs.Value);
                                }
                                
                                using (var performanceMonitor = new PerformanceMonitor())
                                {
                                    await benchMqttClient.Publish($"samples/{clientId}/{hostName}", 256);
                                }
                                
                                var currentCount = Interlocked.Increment(ref messageCount);

                                if(currentCount % reportCount == 0)
                                {
                                    Logger.Info($"Messages sent: '{(((decimal)currentCount / maxMessageCount)).ToString($"0.00%")}' ({messageCount} of {maxMessageCount} messages)");
                                }
                            }

                            Logger.Info($"Client '{clientId}' done sending '{messageByClientCount}' messages");
                        }
                    }, cancellationTokenSource.Token)
                );
            }

            Logger.Info($"Clients created. Press CTRL-C for stop");

            Task.WaitAll(clientTaskList.ToArray(), TimeSpan.FromDays(1));


            Logger.Info($"Publish done.");
            PerfManager.Instance.WriteLogging(Logger);
        }
    }
}