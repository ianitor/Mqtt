// See https://aka.ms/new-console-template for more information

using Ianitor.Common.Configuration;
using MqttBenchmark.Commands;
using MqttBenchmark.Commands.Implementations;
using NLog.Extensions.Logging;
using Ianitor.Common.CommandLineParser;
using Ianitor.Common.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace MqttBenchmark
{
    internal static class Program
    {
        private static async Task<int> Main()
        {
            var logger = LogManager.GetCurrentClassLogger();
            try
            {
                var servicesProvider = BuildDi();
                using (servicesProvider as IDisposable)
                {
                    var runner = servicesProvider.GetRequiredService<Runner>();
                    return await runner.DoActionAsync();
                }
            }
            catch (Exception ex)
            {
                // NLog: catch any exception and log it.
                logger.Error(ex, "Stopped program because of exception");
                return -100;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }
        }

        private static IServiceProvider BuildDi()
        {
            var services = new ServiceCollection();

            // Runner is the custom class
            services.AddTransient<Runner>();

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                        $".{Constants.MqttBenchUserFolderName}{Path.DirectorySeparatorChar}settings.json"),
                    optional: true, reloadOnChange: true)
                .Build();

            services.Configure<MqttBenchOptions>(options =>
                config.GetSection(Constants.MqttBenchRootNode).Bind(options));

            // configure Logging with NLog
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                loggingBuilder.AddNLog(config);
            });

            services.AddSingleton<IConsoleService, ConsoleService>();
            services.AddSingleton<IEnvironmentService, EnvironmentService>();
            services.AddSingleton<IParserService, ParserService>();
            services.AddSingleton<IParser, Parser>();
            services.AddSingleton<IConfigWriter, ConfigWriter>(provider =>
            {
                var configWriter = new ConfigWriter();
                configWriter.AddOptions(Constants.MqttBenchRootNode,
                    provider.GetService<IOptions<MqttBenchOptions>>());

                return configWriter;
            });


            services.AddSingleton<IBenchMqttClient, BenchMqttClient>();
            services.AddTransient<IMqttBenchCommand, PublishCommand>();
            services.AddTransient<IMqttBenchCommand, SubCommand>();

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }
    }
}