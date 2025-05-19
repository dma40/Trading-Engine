using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TradingServer.Core.Configuration;
using TradingServer.Logging;
using TradingServer.Logging.LoggingConfiguration;
using TradingServer.Services;

using Trading;
using Microsoft.Extensions.Configuration;

namespace TradingServer.Core;

internal static class TradingHostBuilder 
{
    public static IHost BuildTradingServer() => Host.CreateDefaultBuilder().ConfigureServices((context, services) =>
    {
        services.AddOptions();
        
        services.Configure<TradingServerConfiguration>(context.Configuration.
                                                    GetSection(nameof(TradingServerConfiguration)));
        services.Configure<LoggerConfiguration>(context.Configuration.
                                                    GetSection(nameof(LoggerConfiguration)));

        var defaultConfig = new LoggerConfiguration
        {
            LoggerType = LoggerType.Text,

            TextLoggerConfiguration = new TextLoggerConfiguration
            {
                Directory = "../Trading-Engine",
                Filename = "TradingLogFile",
                FileExtension = ".log",
            }
        };

        LoggerConfiguration logConfig = context.Configuration.GetSection(nameof(LoggerConfiguration)).
                                        Get<LoggerConfiguration>() ?? defaultConfig;

        if (logConfig.LoggerType == LoggerType.Text)
        {
            services.AddSingleton<ITextLogger, TextLogger>();
        }

        else if (logConfig.LoggerType == LoggerType.Database)
        {
            services.AddSingleton<ITextLogger, DatabaseLogger>();
        }

        else if (logConfig.LoggerType == LoggerType.Console)
        {
            services.AddSingleton<ITextLogger, ConsoleLogger>();
        }

        else if (logConfig.LoggerType == LoggerType.ThreadPoll)
        {
            services.AddSingleton<ITextLogger, ThreadPollLogger>();
        }

        else
        {
            throw new ArgumentException("This is not a supported logger type");
        }
        
        services.AddGrpcClient<TradingClient>(); 
        services.AddSingleton<ITradingServer, TradingServer>();

        services.AddHostedService<TradingServer>();

        services.AddGrpc();
    })
    .Build();
}

