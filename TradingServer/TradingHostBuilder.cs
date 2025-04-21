using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TradingServer.Core.Configuration;
using TradingServer.Logging;
using TradingServer.Logging.LoggingConfiguration;
using TradingServer.Handlers;

using Trading;
using Microsoft.Extensions.Configuration;

namespace TradingServer.Core;

public class TradingHostBuilder 
{
    public static IHost BuildTradingServer() => Host.CreateDefaultBuilder().ConfigureServices((context, services) =>
    {
        services.AddOptions();
        
        services.Configure<TradingServerConfiguration>(context.Configuration.GetSection(nameof(TradingServerConfiguration)));
        services.Configure<LoggerConfiguration>(context.Configuration.GetSection(nameof(LoggerConfiguration)));

        LoggerConfiguration logConfig = context.Configuration.GetSection(nameof(LoggerConfiguration)).Get<LoggerConfiguration>();

        if (logConfig.LoggerType == LoggerType.Text)
        {
            services.AddSingleton<ITextLogger, TextLogger>();
        }

        if (logConfig.LoggerType == LoggerType.Database)
        {
           services.AddSingleton<ITextLogger, DatabaseLogger>();
        }

        if (logConfig.LoggerType == LoggerType.Console)
        {
            services.AddSingleton<ITextLogger, ConsoleLogger>();
        }

        if (logConfig.LoggerType == LoggerType.Trace)
        {
            
        }

        services.AddSingleton<TradingClient>(); 
        services.AddSingleton<ITradingServer, TradingServer>();

        services.AddHostedService<TradingServer>();

        services.AddGrpc();
    })
    .Build();
}

