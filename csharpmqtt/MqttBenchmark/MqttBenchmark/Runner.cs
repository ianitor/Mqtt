using Ianitor.Common.CommandLineParser;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace MqttBenchmark;
internal class Runner
{
    private readonly ILogger<Runner> _logger;
    private readonly IParser _parser;

    public Runner(ILogger<Runner> logger, IParser parser)
    {
        _logger = logger;
        _parser = parser;
    }

    public async Task<int> DoActionAsync()
    {

        try
        {
            _logger.LogInformation("MQTT Benchmark Tool, Version {0}", GetProductVersion());
            _logger.LogInformation(GetCopyright());

            await _parser.ParseAndValidateAsync();

            return 0;
        }
        catch (MandatoryArgumentsMissingException ex)
        {
            _logger.LogError(ex.Message);
            _parser.ShowUsageInformation();
            return -1;
        }
        catch (InvalidProgramException ex)
        {
            _logger.LogError(ex.Message);
            _parser.ShowUsageInformation();
            return -1;
        }
        catch (Exception ex)
        {
            Exception tmp = ex;
            while (tmp != null)
            {
                _logger.LogCritical(tmp, tmp.Message);
                tmp = tmp.InnerException;
            }

            return -99;
        }
    }

    private static string GetProductVersion()
    {
        var attribute = Assembly
            .GetExecutingAssembly()
            .GetCustomAttributes<AssemblyFileVersionAttribute>()
            .Single();
        return attribute.Version;
    }

    private static string GetCopyright()
    {
        var attribute = Assembly
            .GetExecutingAssembly()
            .GetCustomAttributes<AssemblyCopyrightAttribute>()
            .Single();

        return attribute.Copyright;
    }
}