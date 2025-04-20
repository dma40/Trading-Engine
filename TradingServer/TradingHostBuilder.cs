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

        services.AddSingleton<TradingClient>(); 
        services.AddSingleton<ITextLogger, TextLogger>();
        services.AddSingleton<ITradingServer, TradingServer>();

        services.AddHostedService<TradingServer>();

        services.AddGrpc();
    })
    .Build();
}

