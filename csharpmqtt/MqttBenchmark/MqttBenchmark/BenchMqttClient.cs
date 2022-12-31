using System.Text;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using NLog;

namespace MqttBenchmark;

public class BenchMqttClient : IBenchMqttClient
{
    protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly IMqttClient _mqttClient;
    
    private long _messagesReceivedCount;
    
    public long MessagesReceivedCount => _messagesReceivedCount;

    public BenchMqttClient()
    {
        var mqttFactory = new MqttFactory();
        _mqttClient = mqttFactory.CreateMqttClient();
        
        _mqttClient.ApplicationMessageReceivedAsync += args =>
        {
            Interlocked.Increment(ref _messagesReceivedCount);
            
            return Task.CompletedTask;
        };

    }

    public async Task Connect(string address, string clientId)
    {
        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithClientId(clientId)
          //  .WithCredentials("user", "pass")
            .WithTcpServer(address)
            .Build();
        
        await _mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
    }

    public async Task Disconnect()
    {
        await _mqttClient.DisconnectAsync();
    }

    public void Dispose()
    {
        _mqttClient.Dispose();
    }

    public async Task Publish(string topic, int payloadLength)
    {
        var date = DateTime.Now.ToString("O");
        var remainingLength = payloadLength - (date.Length / 16);
        string payload = date.PadLeft(remainingLength / 16, 'a');
        var applicationMessage = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .WithPayload(payload)
            .Build();
        
        var result = await _mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
        if (result.ReasonCode != MqttClientPublishReasonCode.Success)
        {
            Logger.Error($"Result: PacketIdentifier='{result.PacketIdentifier}', ReasonCode='{result.ReasonCode}', ReasonString='{result.ReasonString}'");
        }          
    }

    public async Task Subscribe(string topic)
    {
        var options = new MqttClientSubscribeOptionsBuilder()
            .WithTopicFilter(f => { f.WithTopic(topic); })
            .Build();
        var result = await _mqttClient.SubscribeAsync(options);
        foreach (var mqttClientSubscribeResultItem in result.Items)
        {
            Logger.Info($"Received: ResultCode='{mqttClientSubscribeResultItem.ResultCode}', TopicFilter='{mqttClientSubscribeResultItem.TopicFilter}'");
        }
    }
}   