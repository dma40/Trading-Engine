using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TradingServer.Core.Configuration;
using TradingServer.Logging;
using TradingServer.Handlers;
using Trading;

namespace TradingServer.Core;

public class TradingHostBuilder 
{
    public static IHost BuildTradingServer() => Host.CreateDefaultBuilder().ConfigureServices((context, services) =>
    {
        services.AddOptions();
        services.Configure<TradingServerConfiguration>(context.Configuration.GetSection(nameof(TradingServerConfiguration)));

        services.AddSingleton<ITradingServer, TradingServer>();
        services.AddSingleton<ITextLogger, TextLogger>();

        services.AddHostedService<TradingServer>();

        services.AddGrpc();

        // services.AddSingleton<TradingService>();
    })
    .Build();
}
